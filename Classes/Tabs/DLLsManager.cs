using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using StarZLauncher.Windows;
using static StarZLauncher.Windows.MainWindow;
using StarZLauncher.Classes.Settings;

namespace StarZLauncher.Classes.Tabs
{
    public static class DLLsManager
    {
        private const string DLL_FOLDER = @"StarZ Launcher\DLLs";
        private static readonly ObservableCollection<string> _dlls = new();
        
        public static string? defaultDll;
        // Load the default dll name on launch to display on the mainwindow
        public static void LoadDefaultDLL()
        {
            defaultDll = ConfigTool.GetDefaultDLL();
            DefaultDLLText!.Content = $"Default DLL on launch: {defaultDll}";
        }

        public static void LoadDLLs()
        {
            string dllFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER);

            if (Directory.Exists(dllFolderPath))
            {
                string[] dllFiles = Directory.GetFiles(dllFolderPath, "*.dll");
                foreach (string dllFile in dllFiles)
                {
                    _dlls.Add(Path.GetFileName(dllFile));
                }
            }
            DLLsListManager!.ItemsSource = _dlls;
        }

        public static void SetDefaultDLL()
        {
            string selectedItem = (string)DLLsListManager!.SelectedItem;
            if (selectedItem == null) return;

            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Settings.txt");
            if (!File.Exists(configPath)) return;

            bool? result = StarZMessageBox.ShowDialog($"This is going to set the default DLL to {selectedItem}. It will automatically inject this DLL when launching.", "Warning !");
            if (result == false) return;

            ConfigTool.SetDefaultDLL(selectedItem);
            LoadDefaultDLL();
        }

        public static void Edit()
        {
            string selectedDll = (string)DLLsListManager!.SelectedItem;
            if (selectedDll != null)
            {
                RenameWindow? renameWindow = new(selectedDll);
                BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
                bool? result = renameWindow.ShowDialog();
                if (result == true)
                {
                    string currentName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER, selectedDll);
                    string newName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER, renameWindow.NewNameDLLs);
                    File.Move(currentName, newName);
                    int selectedIndex = DLLsListManager.SelectedIndex;
                    _dlls[selectedIndex] = renameWindow.NewNameDLLs!;
                }
                BackgroundForWindowsOnTop.Visibility = Visibility.Collapsed;
                if (selectedDll == defaultDll)
                {
                    ConfigTool.SetDefaultDLL(renameWindow.NewNameDLLs!);
                    LoadDefaultDLL();
                }
            }
        }

        public static void Reset()
        {
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Settings.txt");
            if (!File.Exists(configPath)) return;

            bool? result = StarZMessageBox.ShowDialog($"This is going to reset the default DLL to NONE. Doing so will remove the auto-injection of your DLL on launch.", "Warning !");
            if (result == false) return;

            ConfigTool.SetDefaultDLL("None");
            LoadDefaultDLL();
        }

        public static void Delete()
        {
            string selectedDll = (string)DLLsListManager!.SelectedItem;
            if (selectedDll != null)
            {
                bool? result = StarZMessageBox.ShowDialog("This is going to delete the DLL you selected. If it was the DLL set as default on launch, it will be reset to NONE.", "Warning !");

                if (result == true)
                {
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER, selectedDll);
                    File.Delete(filePath);

                    _dlls.Remove(selectedDll);
                    if (selectedDll == defaultDll)
                    {
                        ConfigTool.SetDefaultDLL("None");
                        LoadDefaultDLL();
                    }
                }
            }
        }

        public static void Add()
        {
            // Create an OpenFileDialog object to prompt the user to select a DLL file
            OpenFileDialog openFileDialog = new()
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Select a DLL file"
            };

            // Display the OpenFileDialog and wait for the user to select a file
            bool? result = openFileDialog.ShowDialog();

            // Check if the user clicked the OK button in the OpenFileDialog
            if (result == true)
            {
                // Get the path of the selected file
                string selectedFilePath = openFileDialog.FileName;

                // Create the directory where the DLL file will be moved to
                string destinationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher\DLLs\";
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Move the selected DLL file to the destination directory
                string destinationFilePath = destinationDirectory + Path.GetFileName(selectedFilePath);
                File.Move(selectedFilePath, destinationFilePath);

                _dlls.Add(Path.GetFileName(openFileDialog.FileName));
            }
            else
            {
                // The user canceled the operation, do nothing
            }
        }
    }
}
