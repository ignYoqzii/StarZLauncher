using System;
using Windows.Management.Deployment;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;
using StarZLauncher.Windows;
using static StarZLauncher.Windows.MainWindow;
using System.Windows.Media;
using System.IO;
using Windows.System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace StarZLauncher.Classes
{
    public static class PackageHelper
    {
        public const string FAMILY_NAME = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static PackageManager PackageManager { get; } = new();

        public static async Task<AppDiagnosticInfo?> GetPackage()
        {
            try
            {
                // Request diagnostic information for the specified package family name
                var info = await AppDiagnosticInfo.RequestInfoForPackageAsync(FAMILY_NAME);

                // Check if info is not null, has items, and the installed path exists
                if (info != null && info.Count > 0 && Directory.Exists(info[0].AppInfo.Package.InstalledPath))
                {
                    return info[0];
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static async Task<string> GetVersion()
        {
            var minecraftApp = await GetPackage();
            if (minecraftApp == null) return "Unknown";

            var version = minecraftApp.AppInfo.Package.Id.Version;

            // Extract major, minor, and build components
            var major = version.Major;
            var minor = version.Minor;
            var build = version.Build;

            // Get the package version
            string packageVersion = $"{major}.{minor}.{build}";

            // Retrieve the offline mode status
            bool offlineMode = ConfigManager.GetOfflineMode();

            if (!offlineMode)
            {
                // If not in offline mode, attempt to retrieve the game version from JSON
                string gameVersion = await GetGameVersionFromJson(packageVersion);
                return gameVersion ?? packageVersion; // Use packageVersion if gameVersion is null
            }
            else
            {
                // If in offline mode, use the package version
                return packageVersion;
            }
        }

        private static async Task<string> GetGameVersionFromJson(string packageVersion)
        {
            // URL of the GitHub raw JSON file
            string url = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/MinecraftVersionsMapping.json";

            using (var httpClient = new HttpClient())
            {
                string jsonData = await httpClient.GetStringAsync(url);
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

                if (mappings != null && mappings.TryGetValue(packageVersion, out var gameVersion))
                {
                    return gameVersion;
                }
            }
            return packageVersion; // No match found, so show the package version instead
        }

        public static async Task<string?> GetExecutablePath()
        {
            var package = await GetPackage() ?? throw new Exception("Minecraft is not installed.");

            if (!package.AppInfo.Package.IsDevelopmentMode) 
            {
                throw new Exception("Minecraft is installed from either the Xbox App or Microsoft Store. Install it from a custom version switcher.");
            }

            string executablePath = package.AppInfo.Package.InstalledPath;

            try
            {
                // Attempt to access the installation path to check if it's accessible
                if (Directory.Exists(executablePath))
                {
                    // Optionally, try to enumerate files to confirm access rights
                    var files = Directory.GetFiles(executablePath);
                    return executablePath;
                }
                else
                {
                    throw new Exception("Executable path is not accessible.");
                }
            }
            catch (Exception ex)
            {
                // Handle other potential exceptions (e.g., path not found)
                throw new Exception($"Error accessing installation path: {ex.Message}", ex);
            }
        }

        public static void RegisterAppPackage(string packagePath)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    DeveloperModeManager.EnableDeveloperMode();
                }
                catch (Exception ex)
                {
                    StarZMessageBox.ShowDialog($"Closing application to avoid corruption. Try enabling Developer Mode manually. {ex.Message}", "Error !", false);
                    Application.Current.Shutdown();
                }
            });

            var packageUri = new Uri(packagePath);
            var deploymentOperation = PackageManager.RegisterPackageAsync(packageUri, null, DeploymentOptions.DevelopmentMode);

            deploymentOperation.Completed += (asyncInfo, asyncStatus) =>
            {
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        if (asyncStatus == AsyncStatus.Completed)
                        {
                            StarZMessageBox.ShowDialog("App package registered successfully! Don't forget to apply your profile!", "Success !", false);
                            InstallStatusText!.Foreground = Brushes.AliceBlue;
                            InstallStatusText.Text = "";
                            VersionHelper.LoadInstalledMinecraftVersion();
                        }
                        else
                        {
                            StarZMessageBox.ShowDialog($"Failed to register app package. Please enable Developer Mode. For help, join or Discord server.", "Failed !", false);
                            InstallStatusText!.Foreground = Brushes.AliceBlue;
                            InstallStatusText.Text = "";
                        }
                        await MinecraftVersionsListManager.LoadVersionsAsync();
                    });
                });
            };
        }

        public static void UnregisterAppPackage()
        {
            // Have to use this since the program does not run as administrator
            string unregisterCommand = "Get-AppxPackage *minecraftUWP* | Remove-AppxPackage";
            ProcessStartInfo psiUnregister = new("powershell.exe", unregisterCommand)
            {
                Verb = "runas", // Run PowerShell as administrator
                CreateNoWindow = true, // Run without opening a window
                WindowStyle = ProcessWindowStyle.Hidden // Hide the window
            };

            try
            {
                Process.Start(psiUnregister)?.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Handle the case where the user cancels the admin rights prompt
                StarZMessageBox.ShowDialog("Admin rights required to unregister Minecraft package. Closing the launcher to avoid corruption...", "Error !", false);
                Application.Current.Shutdown();
            }
        }
    }
}