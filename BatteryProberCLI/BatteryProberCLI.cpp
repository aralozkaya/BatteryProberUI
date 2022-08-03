/*
	BatteryProberUI - Implementation of the deprecated Battery Prober project using WPF (C#) with a CLI interface (C++)
    Copyright (C) 2022 Ibrahim Aral Ozkaya

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

#include <Windows.h>
#pragma warning(disable : 4996)
#pragma comment(linker,"\"/manifestdependency:type='win32' \
name='Microsoft.Windows.Common-Controls' version='6.0.0.0' \
processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

static int argc;
static LPWSTR* argv;

int WINAPI WinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPSTR lpCmdLine, _In_ int nShowCmd) {
	
	LPCWSTR lpwCmdLine = GetCommandLine();
	argv = CommandLineToArgvW(lpwCmdLine, &argc);

	switch (argc) {
		case 2:
		{
			if (lstrcmp(argv[1], TEXT("/h")) == 0 || lstrcmp(argv[1], TEXT("/?")) == 0|| lstrcmp(argv[1], TEXT("-h")) == 0|| lstrcmp(argv[1], TEXT("-?")) == 0) {
				WCHAR message[] = L"Battery Prober CLI\n\n"
					"\n"
					"ProberCLI.exe <arg1> <arg2>\n"
					"Checks the AC power status, if connected, run arg1; if not, run arg2\n\n"
					"ProberCLI.exe /h\n"
					"Shows this message\n\n"
					"\n"
					"Note that both arg1 and arg2 HAVE TO BE .exe or .bat files\n";
				MessageBox(NULL, message, TEXT("Usage"), MB_ICONQUESTION);
			}
			else {
				MessageBox(NULL, TEXT("Wrong Usage"), TEXT("Error"), MB_ICONERROR);
			}
			break;
		}
		case 3:
		{
			SYSTEM_POWER_STATUS systemPowerStatus = {};
			bool res = GetSystemPowerStatus(&systemPowerStatus);

			WCHAR path[MAX_PATH + 2] = TEXT("");

			if (systemPowerStatus.ACLineStatus == '\x1') {
				wcscat(path, argv[1]);
			}
			else {
				wcscat(path, argv[2]);
			}

			ShellExecute(NULL, TEXT("open"), path, NULL, NULL, nShowCmd);

			break;
		}
		default:
			MessageBox(NULL, TEXT("Wrong Usage"), TEXT("Error"), MB_ICONERROR);
	}

	LocalFree(argv);
	return EXIT_SUCCESS;
}