using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using StarZLauncher.Windows;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes.Settings
{
    public static class VersionHelper
    {
        // class to handle the versions check for the game and the launcher
        // Path to the telemetry_info.json file
        public static string telemetryinfoPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftpe\telemetry_info.json";

        // Version number extracted from the JSON file
        public static string? VersionNumber { get; private set; }
        public static string? LauncherVersion { get; private set; }

        private const string? VERSION_FILE_PATH = @"StarZ Launcher\LauncherVersion.txt";
        private const string? VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";
        private const string? DOWNLOAD_URL = "https://github.com/ignYoqzii/StarZLauncher/releases/download/{0}/StarZLauncher.exe";

        static VersionHelper()
        {
            // get the installed minecraft version
            try
            {
                // Read the JSON file as a string
                string? json = File.ReadAllText(telemetryinfoPath);

                // Parse the JSON string into a JObject
                JObject jObject = JObject.Parse(json);

                // Extract the version number from the "lastsession_Build" property
                VersionNumber = (string?)jObject["lastsession_Build"];
            }
            catch (Exception)
            {
                // Set a default value for the version number or take other appropriate action
                VersionNumber = "Error";
            }

            // get the launcher's version from github
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

        // Updater part of the launcher
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
                bool? result = StarZMessageBox.ShowDialog("A new update is available. Click OK to update the launcher to the latest, or CANCEL to ignore and keep using an outdated version.", "New update available !");

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

        // End of updater part

        public static void LoadCurrentVersions()
        {
            if (telemetryinfoPath != null)
            {
                CurrentMinecraftVersion!.Content = $"{VersionNumber}";
                CurrentLauncherVersion!.Content = $"{LauncherVersion}";
            }
            else
            {
                CurrentMinecraftVersion!.Content = "Error";
                CurrentLauncherVersion!.Content = "Error";
            }
        }
    }
}

