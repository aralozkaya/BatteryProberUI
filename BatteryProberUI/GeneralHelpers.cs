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

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using MicaWPF.Services;
using MicaWPF.Controls;
using System.Windows.Interop;

namespace BatteryProberUI
{
    struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }
    static internal class GeneralHelpers
    {
        [DllImport("Kernel32.dll")]
        unsafe private static extern int GetSystemPowerStatus(SYSTEM_POWER_STATUS* lpSystemPowerStatus);

        static private MainWindow? mWindowInstance;
        static public MainWindow? WindowInstance { 
            get
            {
                return mWindowInstance;
            } 
            set
            {
                if (value == null) return;
                mWindowInstance = value;
                UpdateSchButton();
                UpdateACStatus();
                UpdateResourceColors();
                mWindowInstance.SourceInitialized += WindowInitialized;
            }
        }
        
        static private void WindowInitialized(object? sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(mWindowInstance).Handle;
            HwndSource hsource = HwndSource.FromHwnd(hwnd);
            hsource.AddHook(WndProc);

            if (mWindowInstance == null) return;
            
            var themeService = ThemeService.GetCurrent();
            var currentTheme = ThemeService.GetWindowsTheme();
            themeService.ChangeTheme(currentTheme);

            ListenToAcEvent();
        }
        static public bool CheckTask()
        {
             Process? schTask = Process.Start(new ProcessStartInfo
                                {
                                    Arguments= "/query /tn \"BatteryProber\"",
                                    FileName="schtasks.exe",
                                    CreateNoWindow=true,
                                });
            if (schTask == null)
            return false;

            while (!schTask.HasExited) ;

            if (schTask.ExitCode != 0)
                return false;

            return true;
        }
        
        static public string GetCurrentExe(bool fullPath = true)
        {
            var path = Assembly.GetEntryAssembly()?.GetName().Name + ".exe";
            if (fullPath) return path;
            else return Path.GetFileName(path);
        }
        static public void ElevateAndExit()
        {
            var path = GetCurrentExe();
            Process? elevateProcess;

            try
            {
                elevateProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "runas",
                });
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return;
            }

            // 5 is the exit code for "Access Denied"
            Environment.Exit(5);
        }

        static public bool IsAdmin()
        {
            WindowsPrincipal principal = new(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static public bool UpdateSchButton()
        {
            if (mWindowInstance == null) return false;

            Button schBtn = mWindowInstance.schBtn;

            var taskStatus = CheckTask();
            var oldTag = schBtn.Tag;

            switch (taskStatus)
            {
                case true:
                    schBtn.Content = "Delete Prober Task";
                    schBtn.Tag = 1;
                    break;

                case false:
                    schBtn.Content = "Create Scheduled Task";
                    schBtn.Tag = 0;
                    break;
            }

            if (schBtn.Tag == null || oldTag == schBtn.Tag)
            {
                return false;
            }
            

            return true;
        }

        unsafe static public bool IsAcConnected()
        {
            SYSTEM_POWER_STATUS systemPowerStatus = new();
            _ = GetSystemPowerStatus(&systemPowerStatus);

            return systemPowerStatus.ACLineStatus == 1;
        }

        static public bool ShowMessageBox(string message, string caption, object button, object icon)
        {
            var res = MessageBox.Show(message, caption, (MessageBoxButton)button, (MessageBoxImage)icon);
            return (res == MessageBoxResult.Yes || res == MessageBoxResult.OK);
        }

        static public bool ExtractTemplateXML()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BatteryProberUI.BatteryProbeTaskTemplate.xml";

            Stream? resourceXML = assembly.GetManifestResourceStream(resourceName);
            if (resourceXML == null) return false;

            StreamReader streamReader = new(resourceXML, Encoding.Unicode, true);
            string XMLdata = streamReader.ReadToEnd();
            streamReader.Close();

            StreamWriter streamWriter = new("BatteryProbeTask.xml", false, new UnicodeEncoding(false, true));
            streamWriter.Write(XMLdata);
            streamWriter.Close();

            return true;
        }

        static public bool ExtractCliExe()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BatteryProberUI.BatteryProberCLI.exe";

            Stream? resourceEXE = assembly.GetManifestResourceStream(resourceName);
            if (resourceEXE == null) return false;
            
            BinaryReader binaryReader = new(resourceEXE);
            BinaryWriter binaryWriter = new(File.Open("ProberCLI.exe", FileMode.Create));

            int lenght = (int)binaryReader.BaseStream.Length;
            binaryWriter.Write(binaryReader.ReadBytes(lenght));

            binaryReader.Close();
            binaryWriter.Close();

            return true;
        }
        static public bool CreateTask()
        {
            Process? proc = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/create /tn \"BatteryProber\" /xml \"BatteryProbeTask.xml\"",
                CreateNoWindow = true,
            });
            if (proc == null) return false;

            while (!proc.HasExited) ;

            return proc.ExitCode == 0;
        }

        static public bool DeleteTask()
        {
            Process? proc = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/delete /tn \"BatteryProber\" /f",
                CreateNoWindow = true,
            });
            if (proc == null) return false;

            while (!proc.HasExited) ;

            return proc.ExitCode == 0;
        }

        static public void CopyFileToSystem()
        {
            if (!ExtractCliExe()) return;
            var system32 = Environment.GetEnvironmentVariable("WINDIR") + "\\System32";
            var path = "ProberCLI.exe";
            File.Copy(path, system32 + "\\" + path, true);
        }

        static public void DeleteFileFromSystem()
        {
            var system32 = Environment.GetEnvironmentVariable("WINDIR") + "\\System32";
            var path = "ProberCLI.exe";
            File.Delete(system32 + "\\" + path);
        }

        static public string? OpenExeBatVbs(string caption)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = caption;
            openFileDialog.Filter = "Executable Files|*.exe|Batch Scrips|*.bat|Visual Basic Scripts|*.vbs";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else return null;
        }

        static public void DeleteArtifacts()
        {
            File.Delete("BatteryProbeTask.xml");
            File.Delete("ProberCLI.exe");
        }

        static public void UpdateACStatus()
        {
            if (mWindowInstance == null) return;

            Uri uri;
            if (IsAcConnected())
            {
                uri = new Uri("pack://application:,,,/circle-fill-green.png");
                mWindowInstance.acStatusText.Text = "Plugged In";
            }
            else
            {
                uri = new Uri("pack://application:,,,/circle-fill-red.png");
                mWindowInstance.acStatusText.Text = "Not Plugged In";
            }

            mWindowInstance.acStatusIcon.Source = new BitmapImage(uri);
        }

        public static void ListenToAcEvent()
        {
            EventLog eventLog = new("System")
            {
                Source = "Kernel-Power",
                EnableRaisingEvents = true
            };
            eventLog.EntryWritten += new EntryWrittenEventHandler(OnEntryWritten);
        }

        private static void OnEntryWritten(object source, EntryWrittenEventArgs entryArg)
        {
            if (entryArg.Entry.InstanceId != 105) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateACStatus();
            });
        }

         const int WM_DWMCOMPOSITIONCHANGED = 0x31A;
         const int WM_THEMECHANGED = 0x31E;
         const int WM_SYSCOLORCHANGE = 0x0015;
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg == WM_DWMCOMPOSITIONCHANGED || msg == WM_THEMECHANGED || msg == WM_SYSCOLORCHANGE)
            {
                UpdateResourceColors();
            }
            return IntPtr.Zero;
        }

        private static void UpdateResourceColors()
        {
            if (mWindowInstance == null) return;

            var themeService = ThemeService.GetCurrent();
            var currentTheme = themeService.CurrentTheme;
            if(currentTheme.ToString() == "Light")
            {
                mWindowInstance.refreshBtnImage.Source = new BitmapImage(new Uri("pack://application:,,,/arrow-clockwise.png"));
                mWindowInstance.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/appicon.ico"));
            }
            else
            {
                mWindowInstance.refreshBtnImage.Source = new BitmapImage(new Uri("pack://application:,,,/arrow-clockwise-light.png"));
                mWindowInstance.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/appiconlight.ico"));
            }
        }
    }
}
