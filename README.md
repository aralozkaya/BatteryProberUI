# BatteryProberUI
Implementation of the deprecated Battery Prober project using WPF (C#) with a CLI interface (C++)

BatteryProberUI is a GUI frontend for scheduling commands (.bat, .vbs or .exe files) to be executed when a laptop is (dis)connected to AC power.
While the AC Power status change can be followed via Event Viewer, using this information via Task Manager and checking whether the status change is a "plug" or "unplug" is tedious involving custom event filters.

### UI
BatteryProberUI a WPF application created with a modern look using [MicaWPF](https://github.com/Simnico99/MicaWPF) package.
While this kills the compatibility with Windows 8.1 or lower, it works and creates a modern looking window on Windows 10 or up.

The UI itself is used only for choosing executables, updating an embedded task template XML with given information and finally importing the resulting XML file using **schtasks.exe**

### CLI
The CLI is a stripped down version of the [now-deprecated BatteryProber project](https://github.com/aralozkaya/BatteryProber).
Under normal conditions, the user should not interract or even see the CLI executable, but the UI has a dedicated button for extracting the CLI anyway. The CLI executable is also uploaded to the releases section.

The way the progam works is that when given correct arguments, it calls the Win32 function [GetSystemPowerStatus](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getsystempowerstatus).
The output of this function is a [SYSTEM_POWER_STATUS](https://docs.microsoft.com/en-us/windows/win32/api/winbase/ns-winbase-system_power_status) struct. 
The only value of this struct we need is *ACLineStatus*. If this value is "1", the system is plugged in to AC, and not plugged in if its "0". It can also have a value of "255" for *Unknown Status*, however this is interpreted as **Not Plugged In** by the program.

The CLI application is tested on Windows 7 and up and seems to be working. However, no detailed tests have been done.

## Usage
### UI
The UI is pretty self explanatory
<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)and(height: 250)" srcset="https://user-images.githubusercontent.com/41003972/184226545-526d60b5-58b3-483a-95c4-dae944455db4.png"</source>
    <source media="(prefers-color-scheme: light)and(height: 250)" srcset="https://user-images.githubusercontent.com/41003972/184227709-dc4ef650-a8c8-4a5e-9cb8-40b45f848a99.png"</source>
    <img alt="[Program shown on a plugged in laptop without a currently scheduled task" src="https://user-images.githubusercontent.com/41003972/184227709-dc4ef650-a8c8-4a5e-9cb8-40b45f848a99.png" style="height: 250"/>
  </picture>

  <picture>
    <source media="(prefers-color-scheme: dark)and(height: 250)" srcset="https://user-images.githubusercontent.com/41003972/184227437-483a6be5-094b-4206-af18-1a15549b42fa.png"</source>
    <source media="(prefers-color-scheme: light)and(height: 250)" srcset="https://user-images.githubusercontent.com/41003972/184229552-b4434d6a-e30d-45aa-9d15-b3522f7fe1a4.png"</source>
    <img alt="[Program shown on a currently unplugged laptop without a currently scheduled task" src="https://user-images.githubusercontent.com/41003972/184229552-b4434d6a-e30d-45aa-9d15-b3522f7fe1a4.png" style="height: 250"/>
  </picture>
</div>

At the top, the current power status of the computer is shown, followed by a button to refresh the status, however if the program works correctly, this button is not necessarry since the program listens to the Windows events and if it finds a AC status change event, automatically calls the refresh function.

The second button extracts the CLI application and saves it to the current working directory.

The last button displays a textbox explaining the syntax of the CLI application. Again, under normal conditions user should not be interracting with the CLI application directly.

### CLI
The CLI application takes two arguments *arg1* and *arg2*  

*arg1* is run if the program detects the computer is currently plugged in, and *arg2* is run otherwise.
However, the program is only designed to run *.bat, *.vbs and *.exe files. While running other extensions may be possible, it is not guaranteed and the GUI application does not permit selecting such files.

If the program is run with the single argument of "/h", it displays the same message box as the **About CLI Usage** button, and finally, if the program is run with no or incorrect arguments, it displays an error message

<div align="center">
  <img width="auto" height="250" src="https://user-images.githubusercontent.com/41003972/184234452-4a88a704-1a54-4f92-a21f-fd5d40610921.png" alt="Message box showing the usage of the CLI program">
  <img width="auto" height="250" src="https://user-images.githubusercontent.com/41003972/184235013-e4cf7468-dcf1-4664-a619-1ddf58cba8ee.png" alt="Error message">
</div>

## License
This program is licensed under [GNU General Public License v3.0](https://raw.githubusercontent.com/aralozkaya/BatteryProberUI/main/LICENSE)
