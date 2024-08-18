using StarZLauncher.Windows;
using System;
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

        public static async Task LoadVersionsAsync()
        {
            string versionsUrl = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/MinecraftVersions.txt";
            using HttpClient client = new();
            string versions = await client.GetStringAsync(versionsUrl);
            string[] versionList = versions.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string installationPath = ConfigManager.GetMinecraftInstallationPath();
            string currentVersion = await PackageHelper.GetVersion();

            // Clear existing versions from the stack panel
            if (FullVersionsListStackPanel?.Children.Count > 0)
            {
                FullVersionsListStackPanel.Children.Clear();
            }

            // Create a grid to hold version borders
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Create background colors for alternating rows
            Brush background1 = new SolidColorBrush(Color.FromArgb(255, 36, 44, 53));
            Brush background2 = new SolidColorBrush(Color.FromArgb(255, 23, 29, 34));

            // Add versions to the grid
            for (int i = 0; i < versionList.Length; i++)
            {
                string version = versionList[i];
                string versionFolderPath = Path.Combine(installationPath, $"Minecraft {version}");
                bool isInstalled = Directory.Exists(versionFolderPath);

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
                    Background = (i % 2 == 0) ? background1 : background2,
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(5),
                    BorderThickness = new Thickness(0),
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 20,
                        Opacity = 0.3,
                        RenderingBias = RenderingBias.Performance,
                        ShadowDepth = 5
                    }
                };

                Grid innerGrid = new();
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock textBlock = new()
                {
                    Text = displayVersion,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.AliceBlue),
                    FontFamily = new FontFamily("Outfit"),
                    FontSize = 20,
                    FontWeight = FontWeights.Medium
                };

                Button button = new()
                {
                    Foreground = new SolidColorBrush(Colors.AliceBlue),
                    FontSize = 10,
                    Margin = new Thickness(5, 0, 10, 0),
                    FontWeight = FontWeights.Medium,
                    FontFamily = new FontFamily("Outfit"),
                    Height = 30,
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ProgressBar progressBar = new()
                {
                    Height = 7,
                    Margin = new Thickness(10, 5, 10, 7),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Visibility = Visibility.Collapsed
                };

                LinearGradientBrush gradientBrush = new()
                {
                    StartPoint = new System.Windows.Point(0.5, 0),
                    EndPoint = new System.Windows.Point(0.5, 1)
                };
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0xBC, 0xFB), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x3E, 0x5D), 0.403));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x15, 0x1F), 1));

                progressBar.Foreground = gradientBrush;

                button.Content = version == currentVersion ? "Uninstall" : isInstalled ? "Switch" : "Install";

                button.Click += async (s, e) =>
                {
                    await DownloadVersionAsync(version, progressBar);
                };

                button.Style = (Style)button.FindResource("DefaultDownloadButtons");

                innerGrid.Children.Add(textBlock);
                innerGrid.Children.Add(button);
                innerGrid.Children.Add(progressBar);
                Grid.SetColumn(textBlock, 0);
                Grid.SetColumn(button, 1);
                Grid.SetColumnSpan(progressBar, 2);

                border.Child = innerGrid;

                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) });
                grid.Children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);
                Grid.SetColumnSpan(border, 2);
            }

            FullVersionsListStackPanel?.Children.Add(grid);
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
                        await LoadVersionsAsync();
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
                        await LoadVersionsAsync();
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