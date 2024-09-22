using StarZLauncher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class VersionHelper
    {
        public static string? VersionNumber { get; private set; }

        private const string VERSION_FILE_PATH = @"StarZ Launcher\LauncherVersion.txt";
        private const string VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";
        private const string DOWNLOAD_URL = "https://github.com/ignYoqzii/StarZLauncher/releases/download/{0}/StarZLauncher.exe";

        public static async void CheckForUpdates()
        {
            string currentVersion = GetCurrentVersion();
            string latestVersion = await GetLatestVersion();

            if (currentVersion != latestVersion)
            {
                bool? result = StarZMessageBox.ShowDialog("A new update is available! Click 'OK' to upgrade the launcher to the latest version, or 'CANCEL' to dismiss this notification and continue using the outdated version without future update alerts.", "New update available!");

                if (result == true)
                {
                    string downloadLink = string.Format(DOWNLOAD_URL, latestVersion);
                    DownloadLatestLauncher(downloadLink, latestVersion);
                }
                else 
                {
                    ConfigManager.SetDoNotAskForUpdates(true);
                }
            }
        }

        private static string GetCurrentVersion()
        {
            string versionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VERSION_FILE_PATH);
            return File.Exists(versionFilePath) ? File.ReadAllText(versionFilePath).Trim() : "Unknown";
        }

        private static async Task<string> GetLatestVersion()
        {
            using HttpClient client = new();
            string versionString = await client.GetStringAsync(VERSION_URL);
            return versionString.Trim();
        }

        private static async void DownloadLatestLauncher(string url, string latestVersion)
        {
            string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"StarZ Launcher ({latestVersion}).exe");

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }

            using HttpClient client = new();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Ensure the request was successful

            using (var fs = new FileStream(downloadPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs);
            }

            // Optional: Check if the file was downloaded completely
            if (File.Exists(downloadPath) && new FileInfo(downloadPath).Length > 0)
            {
                // Update the version file
                UpdateVersionFile(latestVersion);

                // Start the new launcher
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadPath,
                    UseShellExecute = true // To ensure it runs properly
                });

                // Give some time for the new process to start
                await Task.Delay(1000);

                // Close the current application
                Application.Current.Shutdown();
            }
            else
            {
                StarZMessageBox.ShowDialog("Failed to download the new launcher. Please try again later.", "Download Error");
            }
        }

        private static void UpdateVersionFile(string latestVersion)
        {
            string versionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VERSION_FILE_PATH);
            File.WriteAllText(versionFilePath, latestVersion);
        }

        public static async void LoadInstalledMinecraftVersion()
        {
            VersionNumber = await PackageHelper.GetVersion();
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentMinecraftVersion!.Content = $"{VersionNumber}";
            });
        }

        public static void LoadCurrentVersions()
        {
            LoadInstalledMinecraftVersion();
            CurrentLauncherVersion!.Content = $"{GetCurrentVersion()}";
        }
    }
}