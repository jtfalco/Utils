using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace DRGSaveFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button[] folderButtons;
        private RadioButton[] radioButtons;
        private const string SteamFolderPrefix = "SteamFolder=", XboxFolderPrefix = "XboxFolder=", BackupFolderPrefix = "BackupFolder=", RadioButtonPrefix = "Behavior=";
        private const string dateTimeFormat = "yyyy-MM-dd HHmmss";
        private readonly string configPath = System.Environment.CurrentDirectory + "\\application.config";
        public MainWindow()
        {
            InitializeComponent();            
            ButtonPickSteamFolder.Click += ButtonPickSteamFolder_OnClick;
            ButtonPickXboxFolder.Click += ButtonPickXboxFolder_OnClick;
            ButtonPickBackupFolder.Click += ButtonPickBackupFolder_OnClick;
            ButtonExecute.Click += ButtonExecute_OnClick;
            TextboxSteamFolder.TextChanged += TextboxSteamFolder_TextChanged;
            TextboxXboxFolder.TextChanged += TextboxXboxFolder_TextChanged;
            TextboxBackupFolder.TextChanged += TextboxBackupFolder_TextChanged;
            folderButtons = new Button[3] { ButtonPickBackupFolder, ButtonPickSteamFolder, ButtonPickXboxFolder };
            radioButtons = new RadioButton[4] { RadioButtonBehavior1, RadioButtonBehavior2, RadioButtonBehavior3, RadioButtonBehavior4 };
            try
            {
                LoadConfigurationFile();
            } catch
            {

            }
            ValidateFolders();
        }

        private void LoadConfigurationFile()
        {            
            using FileStream stream = new FileStream(configPath, FileMode.OpenOrCreate);
            StreamReader reader = new StreamReader(stream);
            string line = string.Empty;            

            while(!reader.EndOfStream && !string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith(SteamFolderPrefix))
                {
                    TextboxSteamFolder.Text = line.Substring(SteamFolderPrefix.Length);
                }
                else if (line.StartsWith(XboxFolderPrefix))
                {
                    TextboxXboxFolder.Text = line.Substring(XboxFolderPrefix.Length);
                }
                else if (line.StartsWith(BackupFolderPrefix))
                {
                    TextboxBackupFolder.Text = line.Substring(BackupFolderPrefix.Length);
                }
                else if (line.StartsWith(RadioButtonPrefix))
                {                    
                    foreach (RadioButton radio in radioButtons)
                    {
                        string text = (string)radio.Content;
                        string configValue = line.Substring(RadioButtonPrefix.Length);
                        if(text == configValue)
                        {
                            radio.IsChecked = true;
                        }
                    }
                }
            }
        }

        private void SaveConfigurationFile()
        {
            if (File.Exists(configPath)) {
                File.Delete(configPath);
            }
            using FileStream stream = new FileStream(configPath, FileMode.Append);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(SteamFolderPrefix + TextboxSteamFolder.Text);
            writer.WriteLine(XboxFolderPrefix + TextboxXboxFolder.Text);
            writer.WriteLine(BackupFolderPrefix + TextboxBackupFolder.Text);
            foreach(RadioButton radio in radioButtons)
            {
                if(radio.IsChecked ?? false)
                {
                    writer.WriteLine(RadioButtonPrefix + radio.Content.ToString());
                }
            }
            writer.Flush();
        }

        private void ButtonPickSteamFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if(ookiiDialog.ShowDialog() == true)
            {
                TextboxSteamFolder.Text = ookiiDialog.SelectedPath;
            }
        }

        private void ButtonPickXboxFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                TextboxXboxFolder.Text = ookiiDialog.SelectedPath;                
            }
        }

        private void ButtonPickBackupFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                TextboxBackupFolder.Text = ookiiDialog.SelectedPath;
            }
        }

        private void ValidateFolders()
        {
            bool validSteam = false, validXbox = false, validBackup = false;
            if (System.IO.Directory.Exists(TextboxSteamFolder.Text))
            {
                RectangleSteam.Visibility = Visibility.Hidden;                
                RectangleSteam.UpdateLayout();
                validSteam = true;
            } else if (RectangleSteam.Visibility == Visibility.Hidden)
            {
                RectangleSteam.Visibility = Visibility.Visible;
                RectangleSteam.UpdateLayout();
            }
            if (System.IO.Directory.Exists(TextboxXboxFolder.Text))
            {
                RectangleXbox.Visibility = Visibility.Hidden;
                RectangleXbox.UpdateLayout();
                validXbox = true;
            }
            else if (RectangleXbox.Visibility == Visibility.Hidden)
            {
                RectangleXbox.Visibility = Visibility.Visible;
                RectangleXbox.UpdateLayout();
            }
            if (System.IO.Directory.Exists(TextboxBackupFolder.Text))
            {
                RectangleBackup.Visibility = Visibility.Hidden;
                RectangleBackup.UpdateLayout();
                validBackup = true;
            }
            else if (RectangleBackup.Visibility == Visibility.Hidden)
            {
                RectangleBackup.Visibility = Visibility.Visible;
                RectangleBackup.UpdateLayout();
            }
            if(validSteam && validXbox && validBackup)
            {
                ButtonExecute.IsEnabled = true;
                ButtonExecute.UpdateLayout();
            } else if (ButtonExecute.IsEnabled)
            {
                ButtonExecute.IsEnabled = false;
                ButtonExecute.UpdateLayout();
            }
        }

        private void TextboxSteamFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();            
        }

        private void TextboxXboxFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();
        }
        
        private void TextboxBackupFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();
        }

        private void ButtonExecute_OnClick(object sender, RoutedEventArgs e)
        {
            foreach(Button button in folderButtons)
            {
                button.IsEnabled = false;
                button.UpdateLayout();
            }

            //Do the things
            try
            {
                SaveConfigurationFile();
            } catch
            {

            }
            BackupFiles(TextboxSteamFolder.Text, TextboxXboxFolder.Text, TextboxBackupFolder.Text);
            string steamPath = TextboxSteamFolder.Text;
            string xboxPath = TextboxXboxFolder.Text;            
            if (RadioButtonBehavior1.IsChecked ?? false)
            {                
                RunCopyNewerOverOlder(steamPath, xboxPath);                
            } else if(RadioButtonBehavior2.IsChecked ?? false)
            {
                RunCopyOlderOverNewer(steamPath, xboxPath);
            } else if (RadioButtonBehavior3.IsChecked ?? false)
            {
                RunCopySteamOverXbox(steamPath, xboxPath);
            } else
            {
                RunCopyXboxOverSteam(steamPath, xboxPath);
            }
            

            foreach (Button button in folderButtons)
            {
                button.IsEnabled = true;
                button.UpdateLayout();
            }
        }

        private void BackupFiles(string steamPath, string xboxPath, string backupPath)
        {
            string time = DateTime.Now.ToString(dateTimeFormat);
            string steamBackupPath = backupPath + "\\" + time + "\\Steam";
            System.IO.Directory.CreateDirectory(steamBackupPath);
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            foreach(string file in steamFiles)
            {
                System.IO.File.Copy(file, steamBackupPath + "\\" + System.IO.Path.GetFileName(file));
            }
            string xboxBackupPath = backupPath + "\\" + time + "\\Xbox";
            System.IO.Directory.CreateDirectory(xboxBackupPath);
            string[] xboxFiles = System.IO.Directory.GetFiles(xboxPath);
            foreach(string file in xboxFiles)
            {
                System.IO.File.Copy(file, xboxBackupPath + "\\" + System.IO.Path.GetFileName(file));
            }
        }

        private void RunCopyXboxOverSteam(string steamPath, string xboxPath)
        {
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            string newFilePath = string.Empty;
            foreach(string file in steamFiles)
            {
                if(!file.Contains("steam_autocloud.vdf"))
                {
                    if(file.Contains("_Player.sav"))
                    {
                        newFilePath = file;
                    }
                    System.IO.File.Delete(file);
                }
            }
            string sourceFilePath = string.Empty;
            string[] xboxFiles = System.IO.Directory.GetFiles(xboxPath);
            foreach(string file in xboxFiles)
            {
                if (!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    sourceFilePath = file;
                }
            }
            System.IO.File.Copy(sourceFilePath, newFilePath);
        }

        private void RunCopySteamOverXbox(string steamPath, string xboxPath)
        {
            string[] xboxFiles = System.IO.Directory.GetFiles(xboxPath);
            string newFilePath = string.Empty;
            foreach (string file in xboxFiles)
            {
                if (!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    newFilePath = file;
                    System.IO.File.Delete(file);
                }
            }
            string sourceFilePath = string.Empty;
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            foreach (string file in steamFiles)
            {
                if (file.Contains("_Player.sav"))
                {
                    sourceFilePath = file;
                }
            }
            System.IO.File.Copy(sourceFilePath, newFilePath);
        }

        private void RunCopyOlderOverNewer(string steamPath, string xboxPath)
        {
            if(IsSteamNewerThanXbox(steamPath, xboxPath))
            {
                RunCopyXboxOverSteam(steamPath, xboxPath);
            } else
            {
                RunCopySteamOverXbox(steamPath, xboxPath);
            }
        }

        private void RunCopyNewerOverOlder(string steamPath, string xboxPath)
        {
            if(IsSteamNewerThanXbox(steamPath, xboxPath))
            {
                RunCopySteamOverXbox(steamPath, xboxPath);
            } else
            {
                RunCopyXboxOverSteam(steamPath, xboxPath);
            }
        }

        private bool IsSteamNewerThanXbox(string steamPath, string xboxPath)
        {
            string pathToSteamSave = string.Empty;
            string pathToXboxSave = string.Empty;
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            foreach(string file in steamFiles)
            {
                if(file.EndsWith("_Player.sav"))
                {
                    pathToSteamSave = file;
                }
            }
            string[] xboxFiles = System.IO.Directory.GetFiles(xboxPath);
            foreach(string file in xboxFiles)
            {
                if(!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    pathToXboxSave = file;
                }
            }
            DateTime timeForSteam = System.IO.File.GetLastWriteTime(pathToSteamSave);
            DateTime timeForXbox = System.IO.File.GetLastWriteTime(pathToXboxSave);
            return timeForSteam > timeForXbox;
        }
    }
}
