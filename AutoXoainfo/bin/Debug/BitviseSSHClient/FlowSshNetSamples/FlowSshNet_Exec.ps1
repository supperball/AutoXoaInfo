# Command line parameters
param([String] $server = "localhost",
      [Int32] $port = 22,
      [Parameter(Mandatory=$True)] [String] $user,
      [Parameter(Mandatory=$True)] [String] $pass,
      [String] $hostKeyFile,
      [String] $hostKeySha256,
      [String] $hostKeyBB,
      [String] $hostKeyMd5,
      [String] $actCode,
      [Parameter(Mandatory=$True)] [String] $cmd
)

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
	{ throw $ph.DescribeConnectError() }

$actionAttempted = $False

# Exec action
if ($cmd.Length -gt 0)
{
	$actionAttempted = $True
	
	# Open Session channel
	$chnl = New-Object Bitvise.FlowSshNet.ClientSessionChannel($client)

	$oph = New-Object Bitvise.FlowSshNet.ProgressHandler
	$chnl.OpenRequest($oph)
	$oph.WaitDone()

	if ($oph.Success)
		{ Write-Host "Session channel opened" }
	else
		{ throw $oph.GetAuxInfo() }

	# Execute Exec request
	$ph = New-Object Bitvise.FlowSshNet.ProgressHandler
	$chnl.ExecRequest($cmd, $ph)
	$ph.WaitDone()
	
	if ($ph.Success)
		{ Write-Host "Remote command '" $cmd "' executed" }
	else
		{ throw $ph.GetAuxInfo() }

    # Read Data from Server
    $rh = New-Object Bitvise.FlowSshNet.ReceiveHandler
    do
	{ 
        $chnl.Receive($rh)
        $rh.WaitDone()
        $data = $rh.GetData()
        if ($data.Length -gt 0)
        {
            $enc = [System.Text.Encoding]::ASCII
            Write-Host -NoNewLine $enc.GetString($data)
        }
    }
    while ($rh.Success -and -not $rh.Eof())

    if (-not $rh.Success)
		{ throw $rh.GetAuxInfo() }

    # Close session channel
	$cph = New-Object Bitvise.FlowSshNet.ProgressHandler
	$chnl.Close($cph)
	$cph.WaitDone()
	
	if ($cph.Success)
		{ Write-Host "Session channel closed" }
	else
		{ throw $cph.GetAuxInfo() }
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
	Write-Host "No remote command provided. Use:"
	Write-Host "  -cmd <remoteCommand> to execute remote command"
}