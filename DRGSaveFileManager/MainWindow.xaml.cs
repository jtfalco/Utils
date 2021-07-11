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
using System.Globalization;
using System.Text.RegularExpressions;

namespace DRGSaveFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button[] folderButtons;
        private RadioButton[] radioButtons;
        private const string SteamFolderPrefix = "SteamFolder=", WindowsAppStoreFolderPrefix = "WindowsAppStoreFolder=", BackupFolderPrefix = "BackupFolder=", RadioButtonPrefix = "Behavior=";
        private const string dateTimeFormat = "yyyy-MM-dd HHmmss";
        private readonly string configPath = System.Environment.CurrentDirectory + "\\application.config";
        private readonly CultureInfo culture = new CultureInfo("en-US");
        private readonly string defaultBackupsFolderName = "DRGBackups";
        private readonly string labelStatusStart = "Status: ";
        public MainWindow()
        {
            InitializeComponent();
            LabelStatus.Content = labelStatusStart + "Application loading...";
            ButtonPickSteamFolder.Click += ButtonPickSteamFolder_OnClick;
            ButtonPickWindowsAppStoreFolder.Click += ButtonPickWindowsAppStoreFolder_OnClick;
            ButtonPickBackupFolder.Click += ButtonPickBackupFolder_OnClick;
            ButtonRecommendWindowsAppStoreFolder.Click += ButtonRecommendWindowsAppStoreFolder_OnClick;
            ButtonRecommendSteamFolder.Click += ButtonRecommendSteamFolder_OnClick;
            ButtonRecommendBackupFolder.Click += ButtonRecommendBackupFolder_Click;
            ButtonExecute.Click += ButtonExecute_OnClick;
            TextboxSteamFolder.TextChanged += TextboxSteamFolder_TextChanged;
            TextboxWindowsAppStoreFolder.TextChanged += TextboxWindowsAppStoreFolder_TextChanged;
            TextboxBackupFolder.TextChanged += TextboxBackupFolder_TextChanged;
            folderButtons = new Button[4] { ButtonPickBackupFolder, ButtonPickSteamFolder, ButtonPickWindowsAppStoreFolder, ButtonRecommendWindowsAppStoreFolder };
            radioButtons = new RadioButton[10] { 
                RadioButtonBehavior1, RadioButtonBehavior2, RadioButtonBehavior3, RadioButtonBehavior4, RadioButtonBehavior5,
                RadioButtonBehavior6, RadioButtonBehavior7, RadioButtonBehavior8, RadioButtonBehavior9, RadioButtonBehavior10
            };
            try
            {
                LoadConfigurationFile();
            } catch
            {

            }
            ValidateFolders();
            LabelStatus.Content = labelStatusStart + "Application loaded.";
        }

        private void LoadConfigurationFile()
        {
            LabelStatus.Content = labelStatusStart + "Loading configuration...";
            using FileStream stream = new FileStream(configPath, FileMode.OpenOrCreate);
            StreamReader reader = new StreamReader(stream);
            string line = string.Empty;            

            while(!reader.EndOfStream && !string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith(SteamFolderPrefix))
                {
                    TextboxSteamFolder.Text = line.Substring(SteamFolderPrefix.Length);
                }
                else if (line.StartsWith(WindowsAppStoreFolderPrefix))
                {
                    TextboxWindowsAppStoreFolder.Text = line.Substring(WindowsAppStoreFolderPrefix.Length);
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
            LabelStatus.Content = labelStatusStart + "Configuration loaded.";
        }

        private void SaveConfigurationFile()
        {
            LabelStatus.Content = labelStatusStart + "Saving configuration file...";
            if (File.Exists(configPath)) {
                File.Delete(configPath);
            }
            using FileStream stream = new FileStream(configPath, FileMode.Append);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(SteamFolderPrefix + TextboxSteamFolder.Text);
            writer.WriteLine(WindowsAppStoreFolderPrefix + TextboxWindowsAppStoreFolder.Text);
            writer.WriteLine(BackupFolderPrefix + TextboxBackupFolder.Text);
            foreach(RadioButton radio in radioButtons)
            {
                if(radio.IsChecked ?? false)
                {
                    writer.WriteLine(RadioButtonPrefix + radio.Content.ToString());
                }
            }
            writer.Flush();
            LabelStatus.Content = labelStatusStart + "Configuration file saved.";
        }

        private void ButtonPickSteamFolder_OnClick(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = "Please select the folder where the Steam version of Deep Rock Galactic puts its save files.";
            var ookiiDialog = new VistaFolderBrowserDialog();
            if(ookiiDialog.ShowDialog() ?? false)
            {
                TextboxSteamFolder.Text = ookiiDialog.SelectedPath;
            }
            LabelStatus.Content = labelStatusStart + "Steam folder set.";
        }

        private void ButtonPickWindowsAppStoreFolder_OnClick(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = "Please select the folder where the Windows App Store version of Deep Rock Galactic puts its save files.";
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() ?? false)
            {
                TextboxWindowsAppStoreFolder.Text = ookiiDialog.SelectedPath;                
            }
            LabelStatus.Content = labelStatusStart + "Windows App Store folder set.";
        }

        private void ButtonPickBackupFolder_OnClick(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = "Please select the folder where backups of your save files should be and go.";
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() ?? false)
            {
                TextboxBackupFolder.Text = ookiiDialog.SelectedPath;
            }
            LabelStatus.Content = labelStatusStart + "Backup folder set.";
        }

        private void ValidateFolders()
        {
            LabelStatus.Content = labelStatusStart + "Validating folder selections...";
            bool validSteam = false, validWindowsAppStore = false, validBackup = false;
            if (System.IO.Directory.Exists(TextboxSteamFolder.Text))
            {
                ValidationRectangleSteam.Visibility = Visibility.Hidden;                
                ValidationRectangleSteam.UpdateLayout();
                validSteam = true;
            } else if (ValidationRectangleSteam.Visibility == Visibility.Hidden)
            {
                ValidationRectangleSteam.Visibility = Visibility.Visible;
                ValidationRectangleSteam.UpdateLayout();
            }
            if (System.IO.Directory.Exists(TextboxWindowsAppStoreFolder.Text))
            {
                ValidationRectangleWindowsAppStore.Visibility = Visibility.Hidden;
                ValidationRectangleWindowsAppStore.UpdateLayout();
                validWindowsAppStore = true;
            }
            else if (ValidationRectangleWindowsAppStore.Visibility == Visibility.Hidden)
            {
                ValidationRectangleWindowsAppStore.Visibility = Visibility.Visible;
                ValidationRectangleWindowsAppStore.UpdateLayout();
            }
            if (System.IO.Directory.Exists(TextboxBackupFolder.Text))
            {
                ValidationRectangleBackup.Visibility = Visibility.Hidden;
                ValidationRectangleBackup.UpdateLayout();
                validBackup = true;
            }
            else if (ValidationRectangleBackup.Visibility == Visibility.Hidden)
            {
                ValidationRectangleBackup.Visibility = Visibility.Visible;
                ValidationRectangleBackup.UpdateLayout();
            }
            if(validSteam && validWindowsAppStore && validBackup)
            {
                ButtonExecute.IsEnabled = true;
                ButtonExecute.UpdateLayout();
            } else if (ButtonExecute.IsEnabled)
            {
                ButtonExecute.IsEnabled = false;
                ButtonExecute.UpdateLayout();
            }
            LabelStatus.Content = labelStatusStart + "Validation complete. Steam folder: " + (validSteam ? "Exists." : "Does not exist.") + " Windows App Store folder: " + (validWindowsAppStore ? "Exists." : "Does not exist.") + " Backup folder: " + (validBackup ? "Exists." : "Does not exist.");
        }

        private void ButtonRecommendBackupFolder_Click(object sender, RoutedEventArgs e)
        {
            RecommendBackupFolderIntoTextbox();
        }

        private void ButtonRecommendSteamFolder_OnClick(object sender, RoutedEventArgs e)
        {
            RecommendSteamFolderIntoTextbox();
        }

        private void ButtonRecommendWindowsAppStoreFolder_OnClick(object sender, RoutedEventArgs e)
        {
            RecommendWindowsAppStoreFolderIntoTextbox();
        }


        private void RecommendBackupFolderIntoTextbox()
        {
            try
            {
                LabelStatus.Content = labelStatusStart + "Recommending Backup folder...";
                string recommendation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).EndWithFolderSeparator();
                if (Directory.Exists(recommendation) && !Directory.Exists(recommendation + defaultBackupsFolderName))
                {
                    Directory.CreateDirectory(recommendation + defaultBackupsFolderName);
                }
                recommendation += defaultBackupsFolderName;
                if (Directory.Exists(recommendation))
                {
                    TextboxBackupFolder.Text = recommendation;
                    TextboxBackupFolder.UpdateLayout();
                    LabelStatus.Content = labelStatusStart + "Backup folder recommended.";
                }
                else
                {
                    LabelStatus.Content = labelStatusStart + "Could not recommend Backup folder.  Make sure to enter an existing folder, use the \"...\" button to the right if you need.";
                }
            } catch (Exception)
            {

            }
        }

        private void RecommendSteamFolderIntoTextbox()
        {
            LabelStatus.Content = labelStatusStart + "Recommending Steam DRG save folder...";
            string recommendation = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Deep Rock Galactic\\FSD\\Saved\\SaveGames\\";            
            if (!Directory.Exists(recommendation))
            {
                recommendation = recommendation.Replace("Program Files (x86)", "Program Files");
            }
            if (Directory.Exists(recommendation))
            {
                TextboxSteamFolder.Text = recommendation;
                TextboxSteamFolder.UpdateLayout();
                LabelStatus.Content = labelStatusStart + "Steam DRG save folder recommended.";
            }
            else
            {
                LabelStatus.Content = labelStatusStart + "Could not find Steam DRG save folder.  Do you have Steam installed somewhere else?  The usual place is, from the base folder for Steam, Steam\\steamapps\\common\\Deep Rock Galactic\\FSD\\Saved\\SaveGames\\";
            }
        }

        private void RecommendWindowsAppStoreFolderIntoTextbox() {
            LabelStatus.Content = labelStatusStart + "Recommending Windows App Store DRG save folder...";
            bool noGoodFolderFound = true;
            string appDataFolderString = Environment.GetEnvironmentVariable("LocalAppData").EndWithFolderSeparator();
            //string appDataFolderString = "%LOCALAPPDATA%".EndWithFolderSeparator();
            string packagesFolderString = appDataFolderString + "Packages";
            string[] DRGAppDataFolderAmongThese = Directory.GetDirectories(packagesFolderString, "*CoffeeStainStudios.DeepRockGalactic*");
            //DirectoryInfo packagesFolderDirectory = new DirectoryInfo(packagesFolderString);
            //DirectoryInfo[] DRGAppDataFolderAmongThese = packagesFolderDirectory.GetDirectories("CoffeeStainStudios\\.DeepRockGalactic");
            if(DRGAppDataFolderAmongThese.Length > 0) {
                string[] SystemAppDataFolderAmongThese = Directory.GetDirectories(DRGAppDataFolderAmongThese[0].EndWithFolderSeparator(),"SystemAppData");
                //DirectoryInfo[] SystemAppDataFolderAmongThese = DRGAppDataFolderAmongThese[0].GetDirectories("SystemAppData");
                if(SystemAppDataFolderAmongThese.Length > 0) {
                    string[] wgsFolderAmongThese = Directory.GetDirectories(SystemAppDataFolderAmongThese[0].EndWithFolderSeparator(),"wgs");
                    //DirectoryInfo[] wgsFolderAmongThese = SystemAppDataFolderAmongThese[0].GetDirectories("wgs");
                    if(wgsFolderAmongThese.Length > 0) {
                        string[] hexMessOneAmongThese = Directory.GetDirectories(wgsFolderAmongThese[0].EndWithFolderSeparator(), "*_*");
                        for(int i = 0; i < hexMessOneAmongThese.Length; i++)
                        {
                            DirectoryInfo checkThis = new DirectoryInfo(hexMessOneAmongThese[i]);
                            if(Regex.IsMatch(checkThis.Name, "[a-fA-F0-9]{16}_[a-fA-F0-9]{32}"))
                            {
                                string[] hexMessTwoAmongThese = Directory.GetDirectories(hexMessOneAmongThese[i].EndWithFolderSeparator(), "*");
                                for(int j = 0; j < hexMessTwoAmongThese.Length; j++)
                                {
                                    DirectoryInfo checkThisToo = new DirectoryInfo(hexMessTwoAmongThese[j]);
                                    if(Regex.IsMatch(checkThisToo.Name, "[a-fA-F0-9]{32}"))
                                    {
                                        TextboxWindowsAppStoreFolder.Text = hexMessTwoAmongThese[j].EndWithFolderSeparator();
                                        TextboxWindowsAppStoreFolder.UpdateLayout();
                                        noGoodFolderFound = false;
                                        LabelStatus.Content = labelStatusStart + "Windows App Store DRG save folder recommended.";
                                    }
                                }
                            }
                        }                        
                    }
                }
            }
            if(noGoodFolderFound)
            {
                LabelStatus.Content = labelStatusStart + "Could not recommend Windows App Store DRG save folder.  Is your AppData folder somewhere unexpected, or, does your Windows App Store DRG save somewhere different than normal?";
            }
        }

        private void TextboxSteamFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();            
        }

        private void TextboxWindowsAppStoreFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();
        }
        
        private void TextboxBackupFolder_TextChanged(object sender, RoutedEventArgs e)
        {
            ValidateFolders();
        }

        private void ButtonExecute_OnClick(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = labelStatusStart + "Disabling buttons while we work...";
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
            string steamPath = TextboxSteamFolder.Text;
            string windowsAppStorePath = TextboxWindowsAppStoreFolder.Text;
            string backupPath = TextboxBackupFolder.Text;            
            bool backedUpTheFiles = BackupFiles(steamPath, windowsAppStorePath, backupPath);
            if (!backedUpTheFiles)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("The file backup was unsuccessful.  The save file that gets replaced will be gone forever if you continue.  Are you sure you wish to continue with the save file replacement?", "No-Backup Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    LabelStatus.Content = labelStatusStart + "Save file management cancelled after backup failed.";
                    return;
                }
                LabelStatus.Content = labelStatusStart + "Continuing file management without backups...";
            } else
            {
                LabelStatus.Content = labelStatusStart + "File backup successful, continuing file management...";
            }
            if (RadioButtonBehavior1.IsChecked ?? false)            
            {
                RunCopyNewerOverOlder(steamPath, windowsAppStorePath);                
            } else if(RadioButtonBehavior2.IsChecked ?? false)
            {
                RunCopyOlderOverNewer(steamPath, windowsAppStorePath);
            } else if (RadioButtonBehavior3.IsChecked ?? false)
            {
                RunCopySteamOverWindowsAppStore(steamPath, windowsAppStorePath);
            } else if(RadioButtonBehavior4.IsChecked ?? false)
            {
                RunCopyWindowsAppStoreOverSteam(steamPath, windowsAppStorePath);
            } else if (RadioButtonBehavior5.IsChecked ?? false)
            { 
                //The next 6 are copying backup folder saves to the regular save locations.
                //The function needs the selected (newest or hand-picked) backup folder, and which of (Steam, Windows App Store) to copy over.
                RunCopyBackupOverSave(steamPath, windowsAppStorePath, GetNewestBackupFolder(backupPath, backedUpTheFiles), true, true);                
            } else if(RadioButtonBehavior6.IsChecked ?? false)
            {
                RunCopyBackupOverSave(steamPath, windowsAppStorePath, GetNewestBackupFolder(backupPath, backedUpTheFiles), true, false);                
            } else if (RadioButtonBehavior7.IsChecked ?? false)
            {
                RunCopyBackupOverSave(steamPath, windowsAppStorePath, GetNewestBackupFolder(backupPath, backedUpTheFiles), false, true);                
            } else if (RadioButtonBehavior8.IsChecked ?? false)
            {
                var ookiiDialog = new VistaFolderBrowserDialog();                
                ookiiDialog.SelectedPath = backupPath.EndWithFolderSeparator();
                if (ookiiDialog.ShowDialog() ?? false)
                {
                    RunCopyBackupOverSave(steamPath, windowsAppStorePath, ookiiDialog.SelectedPath, true, true);
                }
            } else if (RadioButtonBehavior9.IsChecked ?? false)
            {
                var ookiiDialog = new VistaFolderBrowserDialog();
                ookiiDialog.SelectedPath = backupPath.EndWithFolderSeparator();
                if (ookiiDialog.ShowDialog() ?? false)
                {
                    RunCopyBackupOverSave(steamPath, windowsAppStorePath, ookiiDialog.SelectedPath, true, false);
                }
            } else if (RadioButtonBehavior10.IsChecked ?? false)
            {
                var ookiiDialog = new VistaFolderBrowserDialog();
                ookiiDialog.SelectedPath = backupPath.EndWithFolderSeparator();
                if (ookiiDialog.ShowDialog() ?? false)
                {
                    RunCopyBackupOverSave(steamPath, windowsAppStorePath, ookiiDialog.SelectedPath, false, true);
                }
            }

            LabelStatus.Content = labelStatusStart + "File management seems successful. Re-enabling buttons...";
            foreach (Button button in folderButtons)
            {
                button.IsEnabled = true;
                button.UpdateLayout();
            }
            LabelStatus.Content = labelStatusStart + "File management appears to have completed.";
        }

        private string GetNewestBackupFolder(string backupPath, bool backedUpTheFiles)
        {
            string[] allBackupSubfolders = 
                Directory.GetDirectories(backupPath)                
                .OrderByDescending<string, string>(a => a).ToArray<string>();
            int indexOfNewestBackupFolder = 0;
            if (backedUpTheFiles)
            {
                indexOfNewestBackupFolder++;
            }
            string resultingPath = allBackupSubfolders[indexOfNewestBackupFolder]; //cheating for debugs! Wooo! (could just return, but this makes it easier to check)
            return resultingPath;
        }

        private bool BackupFiles(string steamPath, string windowsAppStorePath, string backupPath)
        {
            try
            {
                LabelStatus.Content = labelStatusStart + "Backing up files...";
                string time = DateTime.Now.ToString(dateTimeFormat);
                string steamBackupPath = backupPath.EndWithFolderSeparator() + time + "\\Steam";
                System.IO.Directory.CreateDirectory(steamBackupPath);
                string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
                foreach (string file in steamFiles)
                {
                    System.IO.File.Copy(file, steamBackupPath.EndWithFolderSeparator() + System.IO.Path.GetFileName(file));
                }
                string windowsAppStoreBackupPath = backupPath.EndWithFolderSeparator() + time + "\\Windows App Store";
                System.IO.Directory.CreateDirectory(windowsAppStoreBackupPath);
                string[] windowsAppStoreFiles = System.IO.Directory.GetFiles(windowsAppStorePath);
                foreach (string file in windowsAppStoreFiles)
                {
                    System.IO.File.Copy(file, windowsAppStoreBackupPath.EndWithFolderSeparator() + System.IO.Path.GetFileName(file));
                }
            } catch
            {
                LabelStatus.Content = "File backup failed!";
                return false;
            }
            LabelStatus.Content = labelStatusStart + "File backup succeeded.";
            return true;
        }

        private string DeleteSteamSaveFilesAndGetNewFilePath(string steamPath)
        {
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            string newFilePath = string.Empty;
            foreach (string file in steamFiles)
            {
                if (!file.Contains("steam_autocloud.vdf"))
                {
                    if (file.Contains("_Player.sav"))
                    {
                        newFilePath = file;
                    }
                    System.IO.File.Delete(file);
                }
            }
            return newFilePath;
        }

        private string DeleteWindowsAppStoreSaveFilesAndGetNewFilePath(string windowsAppStorePath)
        {
            string[] windowsAppStoreFiles = System.IO.Directory.GetFiles(windowsAppStorePath);
            string newFilePath = string.Empty;
            foreach (string file in windowsAppStoreFiles)
            {
                if (!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    newFilePath = file;
                    System.IO.File.Delete(file);
                }
            }
            return newFilePath;
        }

        private void RunCopyWindowsAppStoreOverSteam(string steamPath, string windowsAppStorePath)
        {
            string newFilePath = DeleteSteamSaveFilesAndGetNewFilePath(steamPath);
            string sourceFilePath = string.Empty;
            string[] windowsAppStoreFiles = System.IO.Directory.GetFiles(windowsAppStorePath);
            foreach(string file in windowsAppStoreFiles)
            {
                if (!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    sourceFilePath = file;
                }
            }
            System.IO.File.Copy(sourceFilePath, newFilePath);
        }

        private void RunCopySteamOverWindowsAppStore(string steamPath, string windowsAppStorePath)
        {
            string newFilePath = DeleteWindowsAppStoreSaveFilesAndGetNewFilePath(windowsAppStorePath);
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

        private void RunCopyOlderOverNewer(string steamPath, string windowsAppStorePath)
        {
            if(IsSteamNewerThanWindowsAppStore(steamPath, windowsAppStorePath))
            {
                RunCopyWindowsAppStoreOverSteam(steamPath, windowsAppStorePath);
            } else
            {
                RunCopySteamOverWindowsAppStore(steamPath, windowsAppStorePath);
            }
        }

        private void RunCopyNewerOverOlder(string steamPath, string windowsAppStorePath)
        {
            if(IsSteamNewerThanWindowsAppStore(steamPath, windowsAppStorePath))
            {
                RunCopySteamOverWindowsAppStore(steamPath, windowsAppStorePath);
            } else
            {
                RunCopyWindowsAppStoreOverSteam(steamPath, windowsAppStorePath);
            }
        }

        private bool IsSteamNewerThanWindowsAppStore(string steamPath, string windowsAppStorePath)
        {
            string pathToSteamSave = string.Empty;
            string pathToWindowsAppStoreFile = string.Empty;
            string[] steamFiles = System.IO.Directory.GetFiles(steamPath);
            foreach(string file in steamFiles)
            {
                if(file.EndsWith("_Player.sav"))
                {
                    pathToSteamSave = file;
                }
            }
            string[] windowsAppStoreFiles = System.IO.Directory.GetFiles(windowsAppStorePath);
            foreach(string file in windowsAppStoreFiles)
            {
                if(!System.IO.Path.GetFileName(file).StartsWith("container"))
                {
                    pathToWindowsAppStoreFile = file;
                }
            }
            DateTime timeForSteam = System.IO.File.GetLastWriteTime(pathToSteamSave);
            DateTime timeForWindowsAppStore = System.IO.File.GetLastWriteTime(pathToWindowsAppStoreFile);
            return timeForSteam > timeForWindowsAppStore;
        }

        private void RunCopyBackupOverSave(string steamPath, string windowsAppStorePath, string backupPath, bool copySteamBackup, bool copyWindowsAppStoreBackup)
        {
            string copyFromGenericBackupPath = backupPath;
            string copyFromSteamBackupPath = copyFromGenericBackupPath + "\\Steam";
            string copyFromWindowsAppStoreBackupPath = copyFromGenericBackupPath + "\\Windows App Store";
            if(copySteamBackup)
            {
                string newPath = DeleteSteamSaveFilesAndGetNewFilePath(steamPath);
                string[] allFilesInSteamBackupFolder = Directory.GetFiles(copyFromSteamBackupPath);
                string sourcePath = string.Empty;
                foreach(string possibleSourceFile in allFilesInSteamBackupFolder)
                {
                    if(possibleSourceFile.EndsWith("_Player.sav"))
                    {
                        sourcePath = possibleSourceFile;
                    }
                }
                if(string.IsNullOrWhiteSpace(newPath))
                {
                    newPath = steamPath.EndWithFolderSeparator() + System.IO.Path.GetFileName(sourcePath);
                }
                File.Copy(sourcePath, newPath);
            }
            if(copyWindowsAppStoreBackup)
            {
                string newPath = DeleteWindowsAppStoreSaveFilesAndGetNewFilePath(windowsAppStorePath);
                string[] allFilesInWindowsAppStoreBackupFolder = Directory.GetFiles(copyFromWindowsAppStoreBackupPath);
                string sourcePath = string.Empty;
                foreach(string possibleSourceFile in allFilesInWindowsAppStoreBackupFolder)
                {
                    if(!System.IO.Path.GetFileName(possibleSourceFile).StartsWith("container"))
                    {
                        sourcePath = possibleSourceFile;
                    }
                }
                if(string.IsNullOrWhiteSpace(newPath))
                {                   
                    newPath = windowsAppStorePath.EndWithFolderSeparator() + System.IO.Path.GetFileName(sourcePath);                    
                }
                File.Copy(sourcePath, newPath);
            }
        }
    }
    

    public static class StringExtensions 
        {
        public static string EndWithFolderSeparator(this string original, string separator = "\\") {
            return original.EndsWith(separator) ? original : original + separator;
        }
    }
}
