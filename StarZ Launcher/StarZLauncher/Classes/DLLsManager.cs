using Microsoft.Win32;
using StarZLauncher.Windows;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class DLLsManager
    {
        private static readonly string DLL_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"StarZ Launcher\DLLs");
        private static readonly ObservableCollection<string> _dlls = new();
        public static string? DefaultDLL { get; private set; }

        public static void LoadDefaultDLL()
        {
            DefaultDLL = ConfigManager.GetDefaultDLL();
            if (DefaultDLLText != null)
                DefaultDLLText.Content = $"Default DLL on launch: {DefaultDLL}";
        }

        public static void LoadDLLs()
        {
            if (Directory.Exists(DLL_FOLDER))
            {
                var dllFiles = Directory.GetFiles(DLL_FOLDER, "*.dll").Select(Path.GetFileName);
                _dlls.Clear();
                foreach (var dll in dllFiles) _dlls.Add(dll);
            }
            DLLsListManager!.ItemsSource = _dlls;
        }

        public static void SetDefaultDLL()
        {
            if (DLLsListManager!.SelectedItem is not string selectedItem) return;

            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Settings.txt");
            if (!File.Exists(configPath)) return;

            if (StarZMessageBox.ShowDialog($"Set the default DLL to {selectedItem}? It will automatically inject this DLL when launching.", "Warning!") != true) return;

            ConfigManager.SetDefaultDLL(selectedItem);
            LoadDefaultDLL();
        }

        public static void Edit()
        {
            if (DLLsListManager!.SelectedItem is not string selectedDll) return;

            string dllName = Path.GetFileNameWithoutExtension(selectedDll);

            var renameWindow = new RenameWindow(dllName);
            BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;

            if (renameWindow.ShowDialog() == true)
            {
                var currentName = Path.Combine(DLL_FOLDER, selectedDll);
                var newName = Path.Combine(DLL_FOLDER, renameWindow.NewName! + ".dll");
                File.Move(currentName, newName);

                _dlls[DLLsListManager.SelectedIndex] = renameWindow.NewName! + ".dll";
                if (selectedDll == DefaultDLL)
                {
                    ConfigManager.SetDefaultDLL(renameWindow.NewName! + ".dll");
                    LoadDefaultDLL();
                }
            }
            BackgroundForWindowsOnTop.Visibility = Visibility.Collapsed;
        }

        public static void Reset()
        {
            if (StarZMessageBox.ShowDialog("Reset the default DLL to NONE?", "Warning!") != true) return;

            ConfigManager.SetDefaultDLL("None");
            LoadDefaultDLL();
        }

        public static void Delete()
        {
            if (DLLsListManager!.SelectedItem is not string selectedDll) return;

            if (StarZMessageBox.ShowDialog("Delete the selected DLL? If it's the default, it will be reset to NONE.", "Warning!") != true) return;

            File.Delete(Path.Combine(DLL_FOLDER, selectedDll));
            _dlls.Remove(selectedDll);
            if (selectedDll == DefaultDLL)
            {
                ConfigManager.SetDefaultDLL("None");
                LoadDefaultDLL();
            }
        }

        public static void Add()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Select a DLL file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var destinationFilePath = Path.Combine(DLL_FOLDER, Path.GetFileName(openFileDialog.FileName));
                Directory.CreateDirectory(DLL_FOLDER);
                File.Move(openFileDialog.FileName, destinationFilePath);
                _dlls.Add(Path.GetFileName(openFileDialog.FileName));
            }
        }
    }
}