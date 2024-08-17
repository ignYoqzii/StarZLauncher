using Newtonsoft.Json.Linq;
using StarZLauncher.Windows;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class VersionHelper
    {
        // class to handle the versions check for the game and the launcher

        // Version number extracted from the JSON file
        public static string? VersionNumber { get; private set; }
        public static string? LauncherVersion { get; private set; }

        private const string? VERSION_FILE_PATH = @"StarZ Launcher\LauncherVersion.txt";
        private const string? VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";
        private const string? DOWNLOAD_URL = "https://github.com/ignYoqzii/StarZLauncher/releases/download/{0}/StarZLauncher.exe";

        /// <summary>
        /// Launcher Updater code
        /// </summary>
        static VersionHelper()
        {
            string? url = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";

            try
            {
                WebClient client = new();
                var getlauncherVersion = client.DownloadString(url);
                LauncherVersion = getlauncherVersion?.Replace("\n", "");
            }
            catch (Exception)
            {
                LauncherVersion = "Error";
            }
        }

        public static void CheckForUpdates()
        {
            string? currentVersion = GetCurrentVersion();
            string? latestVersion = GetLatestVersion();

            if (currentVersion == latestVersion)
            {
                // Do nothing - launcher is up to date
            }
            else
            {
                bool? result = StarZMessageBox.ShowDialog("A new update is available. Click 'OK' to update the launcher to the latest, or 'CANCEL' to ignore and keep using an outdated version.", "New update available !");

                if (result == true)
                {
                    string downloadLink = string.Format(DOWNLOAD_URL, latestVersion);
                    DownloadLatestLauncher(downloadLink);
                }
            }
        }

        private static string GetCurrentVersion()
        {
            string? versionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VERSION_FILE_PATH);
            return File.ReadAllText(versionFilePath).Trim();
        }

        private static string GetLatestVersion()
        {
            using WebClient client = new();
            string? versionString = client.DownloadString(VERSION_URL);
            return versionString.Trim();
        }

        private static void DownloadLatestLauncher(string url)
        {
            string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            var fileName = $"StarZ Launcher ({LauncherVersion}).exe";
            string filePath = Path.Combine(downloadPath, fileName);

            // Check if the file exists on the desktop and delete it if it does
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Download the latest version of StarZ Launcher
            using WebClient client = new();
            client.DownloadFile(new Uri(url), filePath);

            // Update the version file with the latest version number
            string versionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VERSION_FILE_PATH);
            string latestVersion = client.DownloadString(VERSION_URL).Trim();
            File.WriteAllText(versionFilePath, latestVersion);

            // Close the application
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// End of Launcher Updater code
        /// </summary>

        public static void LoadInstalledMinecraftVersion()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                VersionNumber = await PackageHelper.GetVersion();
                CurrentMinecraftVersion!.Content = $"{VersionNumber}";
            });
        }

        public static void LoadCurrentVersions()
        {
            LoadInstalledMinecraftVersion();

            string currentVersion = GetCurrentVersion();
            CurrentLauncherVersion!.Content = $"{currentVersion}";
        }
    }
}

