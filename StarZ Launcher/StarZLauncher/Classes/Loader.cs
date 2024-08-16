using StarZLauncher.Windows;
using System;
using System.IO;
using System.Windows;
using static StarZLauncher.Classes.MusicPlayer;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{

    /// <summary>
    /// A simple class to load some stuff when the program starts.
    /// </summary>
    public static class Loader
    {

        public static void Load()
        {
            string logFileName = $"AppStartup.txt";

            try
            {
                VersionHelper.LoadCurrentVersions();
                LogManager.Log("Loaded the current installed versions of Minecraft and the launcher.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading current versions: {ex.Message}", logFileName);
            }

            try
            {
                DLLsManager.LoadDefaultDLL();
                LogManager.Log("Default DLL loaded and ready for injection.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading default DLL: {ex.Message}", logFileName);
            }

            try
            {
                ToolsManager.InitializeDragDrop();
                LogManager.Log("Initialized drag and drop for the Utilities tab.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error initializing drag and drop: {ex.Message}", logFileName);
            }

            try
            {
                DLLsManager.LoadDLLs();
                LogManager.Log("Loaded stored DLLs.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading DLLs: {ex.Message}", logFileName);
            }

            try
            {
                CheckForThemes();
                LogManager.Log("Theme loaded.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading the theme: {ex.Message}", logFileName);
            }

            try
            {
                LoadMusicFiles();
                LogManager.Log("Stored music files loaded.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading music files: {ex.Message}", logFileName);
            }

            try
            {
                HardwareMonitor.GetLocalIPAddress();
                LogManager.Log("Retrieved local IP address for displaying.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error retrieving local IP address: {ex.Message}", logFileName);
            }

            bool debug = ConfigManager.GetOfflineMode();
            if (!debug)
            {
                try
                {
                    VersionHelper.CheckForUpdates();
                    LogManager.Log("Checked for updates.", logFileName);
                }
                catch (Exception ex)
                {
                    LogManager.Log($"Error checking for updates: {ex.Message}", logFileName);
                }
            }
            else
            {
                LogManager.Log("Offline mode is enabled. Skipping update check.", logFileName);
            }
        }

        public static void CheckForThemes()
        {
            string Mode = ConfigManager.GetTheme();
            if (Mode == "Light")
            {
                ThemesManager.LoadTheme("LightMode.xaml");
                LightModeCheckBox!.IsChecked = true;
                LightModeCheckBox!.IsEnabled = false;
            }
            else if (Mode == "Dark")
            {
                ThemesManager.LoadTheme("DarkMode.xaml");
                DarkModeCheckBox!.IsChecked = true;
                DarkModeCheckBox!.IsEnabled = false;
            }
        }

        public static void LoadMusicFiles()
        {
            MusicItems.Clear(); // Refreshes the list in case

            string[] musicFiles = Directory.GetFiles(MusicDirectoryPath, "*.mp3");

            foreach (string filePath in musicFiles)
            {
                MusicItem musicItem = new(filePath);
                MusicItems.Add(musicItem);
            }
            CheckForItemsCount();
        }

        private static void CheckForItemsCount()
        {
            if (MusicItems.Count == 0)
            {
                MusicPlayerInformationTextBlock!.Text = "There's nothing here. Add musics to get started!";
                MusicPlayerInformationTextBlock!.Visibility = Visibility.Visible;
            }
            else
            {
                MusicPlayerInformationTextBlock!.Visibility = Visibility.Collapsed;
            }
        }
    }
}
