using System.Windows;
using MicaWPF.Controls;

namespace BatteryProberUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : MicaWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GeneralHelpers.WindowInstance = this;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Name)
            {
                case "refreshBtn":
                    GeneralHelpers.UpdateACStatus();
                    break;

                case "schBtn":
                    {
                        if (!GeneralHelpers.IsAdmin())
                        {
                            string message = "This Action Requires Administrator Privileges.\nWould You like to restart the application in Administrator mode?";
                            string caption = "Administrator Rights Required";
                            if (!GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning)) break;
                            GeneralHelpers.ElevateAndExit();
                            break;
                        }

                        if (!GeneralHelpers.CheckTask())
                        {
                            string? pathIfAC = GeneralHelpers.OpenExeBatVbs("Choose File to Run When AC Connects");
                            if (pathIfAC == null) break;

                            string? pathIfNotAC = GeneralHelpers.OpenExeBatVbs("Choose File to Run When AC Disconnects");
                            if (pathIfNotAC == null) break;

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

                            GeneralHelpers.CopyFileToSystem();
                            GeneralHelpers.CreateTask();

                            string message = "Task Successfully Created";
                            string caption = "Creating Task";
                            GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                            GeneralHelpers.DeleteArtifacts();
                        }

                        else
                        {
                            string message = "Are you sure to delete the task?";
                            string caption = "Deleting Task";
                            if (!GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning)) break;

                            GeneralHelpers.DeleteFileFromSystem();
                            GeneralHelpers.DeleteTask();

                            message = "Task Successfully Deleted";
                            GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        GeneralHelpers.UpdateSchButton();
                        break;
                    }

                case "extBtn":
                    {
                        if (GeneralHelpers.ExtractCliExe())
                        {
                            string message = "ProberCLI.exe has been successfully extracted";
                            string caption = "Success";
                            GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        break;
                    }

                case "usgBtn":
                    { 
                        string caption = "Usage";
                        string message = "Battery Prober CLI\n\n"
                                        +"\n"
                                        +"ProberCLI.exe <arg1> <arg2>\n"
                                        +"Checks the AC power status, if connected, run arg1; if not, run arg2\n\n"
                                        +"ProberCLI.exe /h\n"
                                        +"Shows this message\n\n"
                                        +"\n"
                                        +"Note that both arg1 and arg2 HAVE TO BE .exe or .bat files\n";

                        GeneralHelpers.ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Question);

                        break;
                    }
            }
        }
    }
}
