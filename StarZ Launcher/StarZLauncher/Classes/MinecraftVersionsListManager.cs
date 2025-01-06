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

                Border border = CreateVersionBorder(i, version, currentVersion, isInstalled, out TextBlock textBlock, out TextBlock speedTextBlock, out Button button, out ProgressBar progressBar);

                versionControls.Add((textBlock, button));

                button.Click += async (s, e) =>
                {
                    await DownloadVersionAsync(version, progressBar, speedTextBlock);
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

        private static Border CreateVersionBorder(int index, string version, string currentVersion, bool isInstalled, out TextBlock textBlock, out TextBlock speedTextBlock, out Button button, out ProgressBar progressBar)
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

            // Create the version text block
            textBlock = new TextBlock
            {
                Text = displayVersion,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Outfit"),
                FontSize = 20,
                FontWeight = FontWeights.Medium
            };

            // Create the action button
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

            // Create the progress bar
            progressBar = new ProgressBar
            {
                Height = 7,
                Margin = new Thickness(10, 5, 60, 7),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Visibility = Visibility.Collapsed
            };

            // Create a new TextBlock next to the progress bar to display download speed
            speedTextBlock = new TextBlock
            {
                Text = "00.00 MB/s",
                Margin = new Thickness(665, 0, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontFamily = new FontFamily("Outfit"),
                FontSize = 9,
                FontWeight = FontWeights.Medium,
                Visibility = Visibility.Collapsed
            };

            // Dynamic styles for the color values
            border.Style = (Style)(index % 2 == 0 ? border.FindResource("MinecraftVersionsListPrimaryBorderBackgroundColor") : border.FindResource("MinecraftVersionsListSecondaryBorderBackgroundColor"));
            textBlock.Style = (Style)textBlock.FindResource("MinecraftVersionsListTextBlockColor");
            speedTextBlock.Style = (Style)textBlock.FindResource("MinecraftVersionsListTextBlockColor");
            button.Style = (Style)button.FindResource("DefaultDownloadButtons");
            progressBar.Style = (Style)progressBar.FindResource("MinecraftVersionsListProgressBarColor");

            // Add elements to the grid
            innerGrid.Children.Add(textBlock);
            innerGrid.Children.Add(button);
            innerGrid.Children.Add(progressBar);
            innerGrid.Children.Add(speedTextBlock);

            // Position the elements in the grid
            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(button, 1);
            Grid.SetColumnSpan(progressBar, 2);
            Grid.SetColumnSpan(speedTextBlock, 2);

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

        private static async Task DownloadVersionAsync(string version, ProgressBar progressBar, TextBlock speedTextBlock)
        {
            if (isRunning) // Prevent re-entrant calls
                return;

            string downloadUrl = $"https://github.com/bernarddesfosse/onix_compatible_appx/releases/download/{version}/{version}.appx";
            string folderPath = ConfigManager.GetMinecraftInstallationPath();
            string versionFolderPath = Path.Combine(folderPath, $"Minecraft {version}");
            string zipFilePath = Path.Combine(versionFolderPath, $"{version}.zip");

            isRunning = true;

            try
            {
                // Ensure the directory exists or handle the version switch/uninstall case
                bool continuation = await CheckIfVersionExist(versionFolderPath, version);
                if (!continuation)
                {
                    isRunning = false;
                    return; // Stop and do not start the download process
                }

                // Log download start
                string logFileName = "MinecraftDownload.txt";
                LogManager.Log($"Starting download of Minecraft version {version} from {downloadUrl}.", logFileName);

                // Show the progress bar and speed textblock
                progressBar.Visibility = Visibility.Visible;
                speedTextBlock.Visibility = Visibility.Visible;

                // Download the file
                await DownloadFileAsync(downloadUrl, zipFilePath, progressBar, speedTextBlock, logFileName);

                // Install the downloaded package
                await InstallMinecraftPackage(versionFolderPath, zipFilePath);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error during download: {ex.Message}", "MinecraftDownload.txt"); // Download part is only where it can throw an exception in this case
                StarZMessageBox.ShowDialog($"Failed to download Minecraft version {version}. Please check your Internet connection or try again later.", "Error", false);
                isRunning = false;
                // Hide the progress bar and speed textblock
                progressBar.Visibility = Visibility.Collapsed;
                speedTextBlock.Visibility = Visibility.Collapsed;
                // Delete the folder and its files or subdirectories since the installation failed
                Directory.Delete(versionFolderPath, true);
            }
        }

        private static async Task<bool> CheckIfVersionExist(string versionFolderPath, string version)
        {
            if (!Directory.Exists(versionFolderPath))
            {
                Directory.CreateDirectory(versionFolderPath);
                return true;
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
                    return false;
                }
                else
                {
                    result = StarZMessageBox.ShowDialog($"It appears that Minecraft {version} is already on your computer. If you want to switch to that version, click 'OK'. If you want to uninstall it, press 'CANCEL' or close this window.", "Info", true);

                    if (result == true)
                    {
                        await SwitchMinecraftVersionAsync(versionFolderPath);
                    }
                    else if (result == false)
                    {
                        await DeleteVersionAsync(versionFolderPath);
                        await RefreshVersions();
                        VersionHelper.LoadInstalledMinecraftVersion();
                    }
                    return false;
                }
            }
        }

        private static async Task DownloadFileAsync(string url, string filePath, ProgressBar progressBar, TextBlock speedTextBlock, string logFileName)
        {
            using HttpClient client = new();

            // Send a request to get file size
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            // Ensure the HTTP response is successful
            response.EnsureSuccessStatusCode();

            // Validate content length
            long? totalBytes = response.Content.Headers.ContentLength;
            if (!totalBytes.HasValue || totalBytes.Value <= 0)
            {
                throw new InvalidOperationException("Invalid or missing content length in the HTTP response.");
            }

            progressBar.Maximum = totalBytes.Value;

            // Validate content stream
            using var contentStream = await response.Content.ReadAsStreamAsync();
            if (contentStream == null || !contentStream.CanRead)
            {
                throw new IOException("Unable to read content stream from the HTTP response.");
            }

            // Prepare the file for writing
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bufferedContentStream = new BufferedStream(contentStream, 8192);
            using var bufferedFileStream = new BufferedStream(fileStream, 8192);

            var buffer = new byte[1048576]; // 1MB buffer
            long totalReadBytes = 0;
            int bytesRead;
            int progressUpdateInterval = 10;
            int progressCounter = 0;

            DateTime startTime = DateTime.Now; // Start time for speed calculation

            int speedUpdateInterval = 1000; // Update every 1000 iterations
            int speedUpdateCounter = 0;

            while ((bytesRead = await bufferedContentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // Write the buffer to the file
                await bufferedFileStream.WriteAsync(buffer, 0, bytesRead);
                totalReadBytes += bytesRead;

                // Update progress at intervals
                if (++progressCounter % progressUpdateInterval == 0)
                {
                    progressBar.Value = totalReadBytes;

                    if (totalBytes.HasValue && totalReadBytes > totalBytes.Value)
                    {
                        throw new IOException("Downloaded file size exceeds the expected content length.");
                    }
                }

                // Update speed at intervals
                if (++speedUpdateCounter % speedUpdateInterval == 0)
                {
                    // Calculate download speed
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    if (elapsedTime.TotalSeconds > 0)
                    {
                        double speed = (totalReadBytes) / elapsedTime.TotalSeconds; // bytes per second
                        speedTextBlock.Text = $"{speed / (1024 * 1024):F2} MB/s"; // Convert speed to MB/s
                    }
                }
            }

            // Final validation: Check if the total read bytes match the expected content length
            if (totalReadBytes != totalBytes.Value)
            {
                throw new IOException("Downloaded file size does not match the expected content length.");
            }

            LogManager.Log("Download completed successfully.", logFileName);

            // Ensure the file is properly flushed and closed
            await bufferedFileStream.FlushAsync();
            await fileStream.FlushAsync();

            // Check the final file size on disk
            long finalFileSize = new FileInfo(filePath).Length;
            if (finalFileSize != totalBytes.Value)
            {
                throw new IOException($"Final file size on disk ({finalFileSize} bytes) does not match expected size ({totalBytes.Value} bytes).");
            }

            // Collapse the progress bar and speed text
            progressBar.Visibility = Visibility.Collapsed;
            speedTextBlock.Visibility = Visibility.Collapsed;
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
                    InstallStatusText.Text = $"Registering new version and finalizing Minecraft setup...";
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

                string manifestPath = Path.Combine(versionFolderPath, "AppxManifest.xml");
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