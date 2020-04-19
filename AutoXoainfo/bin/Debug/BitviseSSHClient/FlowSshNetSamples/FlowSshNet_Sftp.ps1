# Command line parameters
param([String] $server = "localhost",
      [Int32]  $port = 22,
      [Parameter(Mandatory=$True)] [String] $user,
      [Parameter(Mandatory=$True)] [String] $pass,
      [String] $hostKeyFile,
      [String] $hostKeySha256,
      [String] $hostKeyBB,
      [String] $hostKeyMd5,
      [String] $actCode,
      [String] $put,
      [String] $get,
      [String] $dest,
      [String] $list)

if ($PSVersionTable.PSVersion.Major -lt 3)
    { throw "This script requires PowerShell version 3.0 or higher." }
 
# Don't continue if something goes wrong.
$ErrorActionPreference = "Stop"

# Check that at least one host key parameter is provided
if (($hostKeyFile.Length -eq 0) -and
	($hostKeySha256.Length -eq 0) -and
	($hostKeyBB.Length -eq 0) -and
	($hostKeyMd5.Length -eq 0))
	{ throw "At least one of -hostKeyFile, -hostKeySha256, -hostKeyBB, or -hostKeyMd5 must be provided" }

# Load .NET assembly
$assembly = $null
if ([Environment]::Is64BitProcess)
    { $assembly = [System.Reflection.Assembly]::LoadWithPartialName("FlowSshNet64") }
else
    { $assembly = [System.Reflection.Assembly]::LoadWithPartialName("FlowSshNet32") }

if ($assembly -eq $null)
    { throw "FlowSshNet assembly not found. Please reinstall Bitvise SSH Client or Bitvise FlowSshNet." }

Write-Host "FlowSshNet assembly loaded: " $assembly " (" $assembly.Location ")"

# Create Client object
$client = New-Object Bitvise.FlowSshNet.Client
$client.SetAppName("PowerShell script")
$client.SetUserName($user)
$client.SetHost($server)
$client.SetPort($port)
$client.SetPassword($pass)

if ($hostKeyFile.Length -gt 0) { $client.AcceptHostKey([System.IO.File]::ReadAllBytes($hostKeyFile)) }
if ($hostKeySha256.Length -gt 0) { $client.AcceptHostKeySha256($hostKeySha256) }
if ($hostKeyBB.Length -gt 0) { $client.AcceptHostKeyBB($hostKeyBB) }
if ($hostKeyMd5.Length -gt 0) { $client.AcceptHostKeyMd5($hostKeyMd5) }

# Connect
$ph = New-Object Bitvise.FlowSshNet.ProgressHandler
$client.Connect($ph)
$ph.WaitDone()

if ($ph.Success)
	{ Write-Host "Connected" }
else
	{ throw $ph.GetAuxInfo() }

$actionAttempted = $False

# SFTP actions
if (($put.Length -gt 0) -or
	($get.Length -gt 0) -or
	($list.Length -gt 0))
{
	$actionAttempted = $True
	
	# Open SFTP channel
	$sftp = New-Object Bitvise.FlowSshNet.ClientSftpChannel($client)
	
	$ph = New-Object Bitvise.FlowSshNet.ProgressHandler
	$sftp.Open($ph)
	$ph.WaitDone()
	
	if ($ph.Success)
		{ Write-Host "SFTP channel opened" }
	else
		{ throw $ph.DescribeSftpChannelOpenError() }
	
	# Run a RealPath request. Some SFTP servers don't work right if this is not done
	$rpWhat = if ($dest.Length -gt 0) { $dest } else { "." }

	$rph = New-Object Bitvise.FlowSshNet.RealPathHandler
	$sftp.RealPath($rpWhat, $rph)
	$rph.WaitDone()
	
	if ($rph.Success)
		{ $remotePath = $rph.GetRealPath() }
	else
		{ throw ("Real path failed: " + $rph.GetError().Describe()) }
	
	# Upload?
	if ($put.Length -gt 0)
	{
		if ($dest.Length -eq 0)
			{ Write-Error "Upload action requires -dest <remotePath>" }
		else
		{
			if (-not (Test-Path $put))
				{ Write-Error "Cannot upload: local path $put does not exist" }
			else
			{
				Write-Host "Uploading $put"

				$th = New-Object Bitvise.FlowSshNet.TransferHandler
				$sftp.Upload($put, $dest, [Bitvise.FlowSshNet.TransferFlags]::Binary, $th)
				$th.WaitDone()
				
				if ($th.Success)
					{ Write-Host "Upload completed" }
				else
					{ Write-Error ("Upload failed: " + $th.GetError().Describe()) }
			}
		}
	}
	
	# Download?
	if ($get.Length -gt 0)
	{
		if ($dest.Length -eq 0)
			{ Write-Error "Download action requires -dest <localPath>" }
		else
		{
			Write-Host "Downloading $get"
			
			$th = New-Object Bitvise.FlowSshNet.TransferHandler
			$sftp.Download($get, $dest, [Bitvise.FlowSshNet.TransferFlags]::Binary, $th)
			$th.WaitDone()
			
			if ($th.Success)
				{ Write-Host "Download completed" }
			else
				{ Write-Error ("Download failed: " + $th.GetError().Describe()) }
		}
	}
	
	# List?
	if ($list.Length -gt 0)
	{
		Write-Host "Listing $list"
		
		$lh = New-Object Bitvise.FlowSshNet.ListHandler($True)
		$sftp.List($list, $lh)
		$lh.WaitDone()
		
		if (-not $lh.Success)
			{ Write-Error ("List failed: " + $lh.GetError().Describe()) }
		else
		{
			foreach ($fileInfo in $lh.GetFileInfos())
			{
				$validFlags = $fileInfo.Attrs.ValidAttrFlags
				
				$fsize = ""
				if (($validFlags -band [Bitvise.FlowSshNet.AttrFlags]::Size) -ne 0)
					{ $fsize = [String]::Format(", size {0}", $fileInfo.Attrs.Size) }
				
				$mtime = ""
				if (($validFlags -band [Bitvise.FlowSshNet.AttrFlags]::ModifyTime) -ne 0)
					{ $mtime = [String]::Format(", mtime {0}", $fileInfo.Attrs.ModifyTime) }
				
				Write-Host ([String]::Format("""{0}"", type {1}{2}{3}", $fileInfo.Name, $fileInfo.Attrs.Type, $fsize, $mtime))
			}

			Write-Host ([String]::Format("{0} entries in listing", $lh.GetFileInfos().Length))
		}
	}
}

# Disconnect
$discph = New-Object Bitvise.FlowSshNet.ProgressHandler
$client.Disconnect($discph)
$discph.WaitDone()

if ($discph.Success)
	{ Write-Host "Disconnected" }
else
	{ throw $discph.GetAuxInfo() }

# No action?
if (-not $actionAttempted)
{
	Write-Host "No action provided. Use:"
	Write-Host "  -put <localPath> to upload"
	Write-Host "  -get <remotePath> to download"
	Write-Host "  -list <remotePath> to list"
}
