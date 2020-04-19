// BvSshCtrl is a command line utility for sending basic commands - such as
// login, logout, exit - to an instance of the graphical Bitvise SSH Client.
// This source file showcases how to send these commands from a C/C++ program.
//
// Copyright (C) 2013-2019 by Bitvise Limited.
//


#pragma warning (disable: 4668)  // L4: 'symbol' is not defined as a preprocessor macro, replacing with '0' for 'directives'
#pragma warning (disable: 4710)  // L4: 'function' : function not inlined
#pragma warning (disable: 4820)  // L4: 'bytes' bytes padding added after construct 'member_name'


// Windows
#include <Windows.h>

// std
#include <string>
#include <iostream>
#include <sstream>
#include <iomanip>


class SystemError : public std::exception
{
public:
	SystemError(char const* function, DWORD error = GetLastError());
	char const* what() const { return m_what.c_str(); }

private:
	std::string m_what;

	class AutoLocalFree
	{
	public:
		AutoLocalFree(HLOCAL mem) : m_mem(mem) {}
		~AutoLocalFree() { LocalFree(m_mem); }
	private:
		HLOCAL m_mem;
	};
};

SystemError::SystemError(char const* function, DWORD error)
{
	using namespace std;

	stringstream ss;
	ss << function;
	ss << " failed: Windows error ";
	if (error <= 99999)
		ss << error;
	else
		ss << "0x" << setfill('0') << hex << setw(8) << error << dec;

	char* message;
	DWORD flags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
	if (!FormatMessageA(flags, 0, error, MAKELANGID(LANG_NEUTRAL, SUBLANG_SYS_DEFAULT), (char*) &message, 0, 0))
		ss << ".";
	else
	{
		AutoLocalFree autoLocalFree(message);
		ss << ": " << message;
	}

	m_what = ss.str();
}


struct EnumWindowsParam
{
	DWORD m_dwProcessId;	// in
	HWND m_hWnd;			// out
};

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam)
{
	EnumWindowsParam& data = *((EnumWindowsParam*) lParam);

	DWORD dwProcessId;
    GetWindowThreadProcessId(hWnd, &dwProcessId);
	if (dwProcessId != data.m_dwProcessId)
		return TRUE;

	if (GetWindowLong(hWnd, GWLP_USERDATA) != 0xB1F7712E) // magic
		return TRUE;

	data.m_hWnd = hWnd;
	SetLastError(ERROR_SUCCESS);
	return FALSE;
}


enum ReturnCode { RCSuccess, RCUsageError, RCInvalidProcessId, RCCommandRejected, RCSystemError, RCException };

int main(int argc, char const* argv[])
{
	using namespace std;

	try
	{
		// Process command-line parameters

		if (argc != 3)
		{
			cout << "Bitvise SSH Client Control Utility" << endl
				 << "Copyright (C) 2012-2019 by Bitvise Limited" << endl
				 << endl
				 << "Usage: BvSshCtrl ProcessID Command" << endl
				 << endl
				 << "ProcessID   Bitvise SSH Client's process ID" << endl
				 << "Command     Currently supported commands: Login, Logout, Exit" << endl;

			return RCUsageError;
		}

		DWORD dwProcessId;
		{
			stringstream ss(argv[1]);
			ss >> dwProcessId;
			if (ss.fail() || !ss.eof())
			{
				cout << "ERROR: ProcessID decode error." << endl;
				return RCUsageError;
			}
		}

		// Find the main window of BvSsh.exe

		EnumWindowsParam param = { dwProcessId };
		if (EnumWindows(EnumWindowsProc, (LPARAM) &param))
		{
			cout << "ERROR: ProcessID is invalid." << endl;
			return RCInvalidProcessId;
		}
		
		if (GetLastError() != ERROR_SUCCESS)
			throw SystemError("EnumWindows()");

		// Send command to BvSsh
		
		COPYDATASTRUCT data;
		data.dwData = 0;
		data.cbData = (DWORD)lstrlenA(argv[2]);
		data.lpData = (void*)argv[2];
		
		if (!SendMessage(param.m_hWnd, WM_COPYDATA, 0, (LPARAM) &data))
		{
			if (GetLastError() != ERROR_SUCCESS)
				throw SystemError("SendMessage()");

			cout << "ERROR: Command rejected." << endl;
			return RCCommandRejected;
		}
	}
	catch (SystemError const& e)
	{
		cout << "ERROR: " << e.what() << endl;
		return RCSystemError;
	}
	catch (std::exception const& e)
	{
		cout << "ERROR: " << e.what() << endl;
		return RCException;
	}

	cout << "Success!" << endl;
	return RCSuccess;
}
