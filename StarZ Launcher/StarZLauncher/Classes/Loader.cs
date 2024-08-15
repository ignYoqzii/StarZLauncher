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
            VersionHelper.LoadCurrentVersions();
            DLLsManager.LoadDefaultDLL();
            ToolsManager.InitializeDragDrop();
            DLLsManager.LoadDLLs();
            HardwareMonitor.GetLocalIPAddress();

            bool Debug = ConfigManager.GetOfflineMode();
            if (Debug == false)
            {
                VersionHelper.CheckForUpdates();
            }
            else if (Debug == true)
            {
                return;
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
                MusicItem musicItem = new MusicItem(filePath);
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
