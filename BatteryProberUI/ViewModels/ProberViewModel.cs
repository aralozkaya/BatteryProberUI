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
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MicaWPF.Services;

namespace BatteryProberUI
{
    public class ProberViewModel : BaseViewModel
    {
        private MainWindow mainWindow { get; set; }
        public string AcStatusText { get; set; }
        public string SchBtnText { get; set; }
        public ImageSource RefreshBtnImage { get; set; }
        public ImageSource AcStatusIcon { get; set; }
        public RelayCommand RefreshBtnCommand { get; set; }
        public RelayCommand SchBtnCommand { get; set; }
        public RelayCommand ExtractBtnCommand { get; set; }
        public RelayCommand UsgBtnCommand { get; set; }
        public ProberViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindow.Loaded += OnInitialize;

            AcStatusText = SystemPowerStatus.IsAcConnected() ? "Plugged In" : "Not Plugged In";

            AcStatusIcon = SystemPowerStatus.IsAcConnected() ? new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-green.png"))
                                                             : new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-red.png"));
            SchBtnText = GeneralHelpers.CheckTask() ? "Delete Prober Task" : "Schedule Prober Task";

            RefreshBtnImage = IsLightTheme() ? new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/arrow-clockwise.png"))
                                             : new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/arrow-clockwise-light.png"));


            SchBtnCommand = GeneralHelpers.CheckTask() ? new RelayCommand(Delete) : new RelayCommand(Schedule);

            ExtractBtnCommand = new RelayCommand(ExtractCLI);

            UsgBtnCommand = new RelayCommand(Usage);

            RefreshBtnCommand = new RelayCommand(Refresh);
        }
        private bool IsLightTheme()
        {
            var themeService = ThemeService.GetCurrent();
            return themeService.CurrentTheme == MicaWPF.WindowsTheme.Light;
        }

        private void Schedule()
        {
            if (!GeneralHelpers.IsAdmin())
            {
                string mes = "This Action Requires Administrator Privileges.\nWould You like to restart the application in Administrator mode?";
                string cap = "Administrator Rights Required";

                if (MessageBox.Show(mes, cap, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                GeneralHelpers.ElevateAndExit();
            }

            GeneralHelpers.ExtractCliExe();
            GeneralHelpers.CopyFileToSystem();
            GeneralHelpers.ExtractTemplateXML();
            
            string? pathIfAC = GeneralHelpers.OpenExeBatVbs("Choose File to Run When AC Connects");
            if (pathIfAC == null) return;

            string? pathIfNotAC = GeneralHelpers.OpenExeBatVbs("Choose File to Run When AC Disconnects");
            if (pathIfNotAC == null) return;

            GeneralHelpers.ExtractTemplateXML();
            XmlHelper xmlDoc = new("BatteryProbeTask.xml");
            MyXmlNode root = xmlDoc.root;

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            MyXmlNode authorNode = root.GetChildrenByName("Task")[0].GetChildrenByName("RegistrationInfo")[0].GetChildrenByName("Author")[0];
            if (authorNode.Children != null) authorNode = authorNode.Children[0];
            authorNode.Value = userName;

            string args = "\"" + pathIfAC + "\" " + "\"" + pathIfNotAC + "\"";
            MyXmlNode argumentNode = root.GetChildrenByName("Task")[0].GetChildrenByName("Actions")[0].GetChildrenByName("Exec")[0].GetChildrenByName("Arguments")[0];
            if (argumentNode.Children != null) argumentNode = argumentNode.Children[0];
            argumentNode.Value = args;

            xmlDoc.SaveXml();

            GeneralHelpers.CreateTask();

            string message = "Task Successfully Created";
            string caption = "Creating Task";

            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            
            GeneralHelpers.DeleteArtifacts();

            SchBtnText = GeneralHelpers.CheckTask() ? "Delete Prober Task" : "Schedule Prober Task";
            SchBtnCommand = GeneralHelpers.CheckTask() ? new RelayCommand(Delete) : new RelayCommand(Schedule);
        }

        private void Delete()
        {
            if (!GeneralHelpers.IsAdmin())
            {
                string mes = "This Action Requires Administrator Privileges.\nWould You like to restart the application in Administrator mode?";
                string cap = "Administrator Rights Required";

                if (MessageBox.Show(mes, cap, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                GeneralHelpers.ElevateAndExit();
            }

            string message = "Are you sure to delete the task?";
            string caption = "Deleting Task";

            MessageBoxResult res = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            GeneralHelpers.DeleteFileFromSystem();
            GeneralHelpers.DeleteTask();

            message = "Task Successfully Deleted";
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

            SchBtnText = GeneralHelpers.CheckTask() ? "Delete Prober Task" : "Schedule Prober Task";
            SchBtnCommand = GeneralHelpers.CheckTask() ? new RelayCommand(Delete) : new RelayCommand(Schedule);
        }

        private void ExtractCLI()
        {
            if (GeneralHelpers.ExtractCliExe())
            {
                string message = "ProberCLI.exe has been successfully extracted";
                string caption = "Success";

                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else
            {
                string message = "Error When Extracting ProberCLI.exe";
                string caption = "Error";

                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Usage()
        {
            string caption = "Usage";
            string message = "Battery Prober CLI\n\n"
                            + "\n"
                            + "ProberCLI.exe <arg1> <arg2>\n"
                            + "Checks the AC power status, if connected, run arg1; if not, run arg2\n\n"
                            + "ProberCLI.exe /h\n"
                            + "Shows this message\n\n"
                            + "\n"
                            + "Note that both arg1 and arg2 HAVE TO BE .exe or .bat files\n";

            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void Refresh()
        {
            AcStatusText = SystemPowerStatus.IsAcConnected() ? "Plugged In" : "Not Plugged In";

            SchBtnText = GeneralHelpers.CheckTask() ? "Delete Prober Task" : "Schedule Prober Task";

            AcStatusIcon = SystemPowerStatus.IsAcConnected() ? new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-green.png"))
                                                             : new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-red.png"));
        }

        private void OnInitialize(object sender, EventArgs e)
        {
            var themeService = ThemeService.GetCurrent();
            var currentTheme = ThemeService.GetWindowsTheme();
            themeService.ChangeTheme(currentTheme);

            UpdateResourceColors();

            IntPtr hwnd = new WindowInteropHelper(mainWindow).Handle;
            HwndSource hsource = HwndSource.FromHwnd(hwnd);
            hsource.AddHook(WndProc);

            ListenToAcEvent();
        }

        private void UpdateResourceColors()
        {
            if (mainWindow == null) return;

            if (IsLightTheme())
            {
                RefreshBtnImage = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/arrow-clockwise.png"));
                mainWindow.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icons/appicon.ico"));
            }
            else
            {
                RefreshBtnImage = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/arrow-clockwise-light.png"));
                mainWindow.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Icons/appiconlight.ico"));
            }
        }

        private const int WM_DWMCOMPOSITIONCHANGED = 0x31A;
        private const int WM_THEMECHANGED = 0x31E;
        private const int WM_SYSCOLORCHANGE = 0x0015;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DWMCOMPOSITIONCHANGED || msg == WM_THEMECHANGED || msg == WM_SYSCOLORCHANGE)
            {
                UpdateResourceColors();
            }
            return IntPtr.Zero;
        }

        public void ListenToAcEvent()
        {
            EventLog eventLog = new("System")
            {
                Source = "Kernel-Power",
                EnableRaisingEvents = true
            };
            eventLog.EntryWritten += new EntryWrittenEventHandler(OnEntryWritten);
        }

        private void OnEntryWritten(object source, EntryWrittenEventArgs entryArg)
        {
            if (entryArg.Entry.InstanceId != 105) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                AcStatusText = SystemPowerStatus.IsAcConnected() ? "Plugged In" : "Not Plugged In";

                AcStatusIcon = SystemPowerStatus.IsAcConnected() ? new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-green.png"))
                                                                 : new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/circle-fill-red.png"));
            }); 
        }
    }
}
