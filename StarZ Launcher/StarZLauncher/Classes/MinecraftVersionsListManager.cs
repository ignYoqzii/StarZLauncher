using StarZLauncher.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public class MinecraftVersionsListManager
    {
        private static bool isRunning = false;

        public static async void LoadVersionsManager()
        {
            await LoadVersionsAsync();
        }

        private static readonly List<(TextBlock TextBlock, Button Button)> versionControls = new();
        private static string[] versionList = Array.Empty<string>(); // Store the version list

        public static async Task LoadVersionsAsync()
        {
            string versionsUrl = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/MinecraftVersions.txt";
            using HttpClient client = new();
            string versions = await client.GetStringAsync(versionsUrl);
            versionList = versions.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string installationPath = ConfigManager.GetMinecraftInstallationPath();
            string currentVersion = await PackageHelper.GetVersion();

            // Clear existing versions from the stack panel
            FullVersionsListStackPanel?.Children.Clear();

            // Create a grid to hold version borders
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            versionControls.Clear(); // Clear previous controls

            // Add versions to the grid
            for (int i = 0; i < versionList.Length; i++)
            {
                string version = versionList[i];
                string versionFolderPath = Path.Combine(installationPath, $"Minecraft {version}");
                bool isInstalled = Directory.Exists(versionFolderPath);

                Border border = CreateVersionBorder(i, version, currentVersion, isInstalled, out TextBlock textBlock, out Button button, out ProgressBar progressBar);

                versionControls.Add((textBlock, button));

                button.Click += async (s, e) =>
                {
                    await DownloadVersionAsync(version, progressBar);
                };

                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) });
                grid.Children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);
                Grid.SetColumnSpan(border, 2);
            }

            FullVersionsListStackPanel?.Children.Add(grid);

            // Initial update of the controls
            UpdateVersionControls(currentVersion);
        }

        private static Border CreateVersionBorder(int index, string version, string currentVersion, bool isInstalled,
                                                   out TextBlock textBlock, out Button button, out ProgressBar progressBar)
        {
            string displayVersion = version;
            if (version == currentVersion)
            {
                displayVersion += " - In use";
            }
            else if (isInstalled)
            {
                displayVersion += " - Downloaded";
            }

            Border border = new()
            {
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(5),
                BorderThickness = new Thickness(0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 20,
                    Opacity = 0.3,
                    RenderingBias = RenderingBias.Performance,
                    ShadowDepth = 5
                },
            };

            Grid innerGrid = new();
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            textBlock = new TextBlock
            {
                Text = displayVersion,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Outfit"),
                FontSize = 20,
                FontWeight = FontWeights.Medium
            };

            button = new Button
            {
                Foreground = new SolidColorBrush(Colors.AliceBlue),
                FontSize = 10,
                Margin = new Thickness(5, 0, 10, 0),
                FontWeight = FontWeights.Medium,
                FontFamily = new FontFamily("Outfit"),
                Height = 30,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Content = version == currentVersion ? "Uninstall" : isInstalled ? "Switch" : "Install"
            };

            progressBar = new ProgressBar
            {
                Height = 7,
                Margin = new Thickness(10, 5, 10, 7),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Visibility = Visibility.Collapsed
            };

            // Dynamic styles for the color values
            border.Style = (Style)(index % 2 == 0 ? border.FindResource("MinecraftVersionsListPrimaryBorderBackgroundColor") : border.FindResource("MinecraftVersionsListSecondaryBorderBackgroundColor"));

            textBlock.Style = (Style)textBlock.FindResource("MinecraftVersionsListTextBlockColor");

            button.Style = (Style)button.FindResource("DefaultDownloadButtons");

            progressBar.Style = (Style)progressBar.FindResource("MinecraftVersionsListProgressBarColor");

            innerGrid.Children.Add(textBlock);
            innerGrid.Children.Add(button);
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(button, 1);
            Grid.SetColumnSpan(progressBar, 2);
            innerGrid.Children.Add(progressBar);

            border.Child = innerGrid;

            return border;
        }

        private static void UpdateVersionControls(string currentVersion)
        {
            for (int i = 0; i < versionControls.Count; i++)
            {
                string version = versionList[i];
                string versionFolderPath = Path.Combine(ConfigManager.GetMinecraftInstallationPath(), $"Minecraft {version}");
                bool isDownloaded = Directory.Exists(versionFolderPath);
                var (textBlock, button) = versionControls[i];

                // Update text block
                string displayVersion = version;
                if (version == currentVersion)
                {
                    displayVersion += " - In use";
                }
                else
                {
                    if (isDownloaded)
                    {
                        displayVersion += " - Downloaded";
                    }
                }
                textBlock.Text = displayVersion;

                // Update button content based on the current version and isDownloaded status

                if (version == currentVersion)
                {
                    button.Content = "Uninstall";
                }
                else if (isDownloaded)
                {
                    button.Content = "Switch";
                }
                else
                {
                    button.Content = "Install";
                }
            }
        }

        public static async Task RefreshVersions()
        {
            // Get the current version again
            string currentVersion = await PackageHelper.GetVersion();
            UpdateVersionControls(currentVersion);
        }

        private static async Task DownloadVersionAsync(string version, ProgressBar progressBar)
        {

            if (isRunning) // check if installation is already in progress or not
            {
                return;
            }

            string downloadUrl = $"https://github.com/bernarddesfosse/onix_compatible_appx/releases/download/{version}/{version}.appx";
            string folderPath = ConfigManager.GetMinecraftInstallationPath();
            string versionFolderPath = Path.Combine(folderPath, $"Minecraft {version}");
            string zipFilePath = Path.Combine(versionFolderPath, $"{version}.zip");

            isRunning = true;
            // Ensure the directory exists or handle the version switch/uninstall case
            if (!Directory.Exists(versionFolderPath))
            {
                Directory.CreateDirectory(versionFolderPath);
            }
            else
            {
                string currentVersion = await PackageHelper.GetVersion();
                bool? result;

                if (currentVersion == version)
                {
                    result = StarZMessageBox.ShowDialog($"Minecraft {version} will be uninstalled from your computer. Click 'OK' to continue or 'CANCEL' if you changed your mind.", "Info", true);

                    if (result == true)
                    {
                        await UnregisterAndDeleteVersionAsync(versionFolderPath);
                        await RefreshVersions();
                        VersionHelper.LoadInstalledMinecraftVersion();
                    }
                    isRunning = false;
                    return;
                }
                else
                {
                    result = StarZMessageBox.ShowDialog($"It appears that Minecraft {version} is already on your computer. If you want to switch to that version, click 'OK'. If you want to uninstall it, press 'CANCEL' or close this window.", "Info", true);

                    if (result == true)
                    {
                        await SwitchMinecraftVersionAsync(versionFolderPath);
                        isRunning = false;
                    }
                    else if (result == false)
                    {
                        await DeleteVersionAsync(versionFolderPath);
                        await RefreshVersions();
                        VersionHelper.LoadInstalledMinecraftVersion();
                        isRunning = false;
                    }
                    return;
                }
            }

            // Download and install the new version
            progressBar.Visibility = Visibility.Visible;

            using WebClient client = new();
            client.DownloadProgressChanged += (s, e) => progressBar.Value = e.ProgressPercentage;

            await client.DownloadFileTaskAsync(downloadUrl, zipFilePath);

            progressBar.Visibility = Visibility.Collapsed;

            await InstallMinecraftPackage(versionFolderPath, zipFilePath);
        }

        private static async Task UnregisterAndDeleteVersionAsync(string versionFolderPath)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Yellow;
                    InstallStatusText.Text = "Unregistering and deleting Minecraft files...";
                });

                PackageHelper.UnregisterAppPackage();
                Directory.Delete(versionFolderPath, true);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.AliceBlue;
                    InstallStatusText.Text = "";
                });
            });
        }

        private static async Task DeleteVersionAsync(string versionFolderPath)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Yellow;
                    InstallStatusText.Text = "Deleting Minecraft files...";
                });
                Directory.Delete(versionFolderPath, true);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.AliceBlue;
                    InstallStatusText.Text = "";
                });
            });
        }

        private static async Task SwitchMinecraftVersionAsync(string versionFolderPath)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Red;
                    InstallStatusText.Text = "Unregistering current Minecraft version...";
                });
                PackageHelper.UnregisterAppPackage();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Green;
                    InstallStatusText.Text = $"Registering and finalizing Minecraft setup...";
                });

                string packagePath = Path.Combine(versionFolderPath, "AppxManifest.xml");
                PackageHelper.RegisterAppPackage(packagePath);

                VersionHelper.LoadInstalledMinecraftVersion();
            });
        }

        public static async Task InstallMinecraftPackage(string versionFolderPath, string zipFilePath)
        {
            // Show warning message box to the user
            bool? result = StarZMessageBox.ShowDialog("This will uninstall your current Microsoft Store Minecraft version. You will be prompted to back-up your com.mojang folder if you continue.", "Warning !");

            if (result == false)
            {
                return; // user clicked cancel, so do nothing
            }

            await ToolsManager.SaveProfile(); // ask the user to save the current profile before uninstalling

            await Task.Run(() =>
            {
                // Unregister Minecraft from Microsoft Store
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Red;
                    InstallStatusText.Text = "Unregistering Minecraft...";
                });
                PackageHelper.UnregisterAppPackage();

                // Update UI to show "Extracting..." and ensure it has time to refresh
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Yellow;
                    InstallStatusText.Text = "Extracting...";
                });

                // Extract the downloaded version file
                ZipFile.ExtractToDirectory(zipFilePath, versionFolderPath);

                // Delete AppxSignature.p7x so that the game can be installed with developer mode
                var signature = System.IO.Path.Combine(versionFolderPath, "AppxSignature.p7x");
                if (File.Exists(signature))
                {
                    File.Delete(signature);
                }

                // Register the app using sideloading method
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Green;
                    InstallStatusText.Text = "Registering and finalizing Minecraft installation...";
                });

                string manifestPath = System.IO.Path.Combine(versionFolderPath, "AppxManifest.xml");
                PackageHelper.RegisterAppPackage(manifestPath);

                // Delete the initial zip file if it still exists
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            });

            isRunning = false; // Reset installation in progress flag
        }
    }
}