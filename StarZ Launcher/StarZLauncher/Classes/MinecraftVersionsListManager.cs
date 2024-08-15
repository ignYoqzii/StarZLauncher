using Microsoft.Win32;
using StarZLauncher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Windows.Foundation;
using Windows.Management.Deployment;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class MinecraftVersionsListManager
    {
        private static bool isRunning = false;
        static MinecraftVersionsListManager()
        {
        }

        public static async void LoadVersionsManager()
        {
            await LoadVersionsAsync();
        }

        private static async Task LoadVersionsAsync()
        {
            string versionsUrl = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/MinecraftVersions.txt";
            using WebClient client = new();
            string versions = await client.DownloadStringTaskAsync(versionsUrl);
            string[] versionList = versions.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

                // Create a border with rounded corners
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

                // Create a grid inside the border for version details
                Grid innerGrid = new();
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                // Create a text block with the version number
                TextBlock textBlock = new()
                {
                    Text = version,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.AliceBlue),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };

                // Create a button to download the version
                Button button = new()
                {
                    Content = "Download",
                    Foreground = new SolidColorBrush(Colors.AliceBlue),
                    FontSize = 10,
                    Margin = new Thickness(5, 0, 10, 0),
                    FontWeight = FontWeights.Bold,
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

                // Create a linear gradient brush
                LinearGradientBrush gradientBrush = new()
                {
                    StartPoint = new System.Windows.Point(0.5, 0),
                    EndPoint = new System.Windows.Point(0.5, 1)
                };
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0xBC, 0xFB), 0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x3E, 0x5D), 0.403));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x15, 0x1F), 1));

                // Set the linear gradient brush as the foreground of the progress bar
                progressBar.Foreground = gradientBrush;

                button.Click += async (s, e) =>
                {
                    progressBar.Visibility = Visibility.Visible;
                    await DownloadVersionAsync(version, progressBar);
                };
                button.Style = (Style)button.FindResource("DefaultDownloadButtons");

                // Add the text block, button, and progress bar to the inner grid
                innerGrid.Children.Add(textBlock);
                innerGrid.Children.Add(button);
                innerGrid.Children.Add(progressBar);
                Grid.SetColumn(textBlock, 0);
                Grid.SetColumn(button, 1);
                Grid.SetColumnSpan(progressBar, 2);

                // Add the inner grid to the border
                border.Child = innerGrid;

                // Add the border to the main grid
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) }); // Adjust the height as needed
                grid.Children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);
                Grid.SetColumnSpan(border, 2);
            }

            // Add the grid to the stack panel
            FullVersionsListStackPanel?.Children.Add(grid);
        }


        private static async Task DownloadVersionAsync(string version, ProgressBar progressBar)
        {
            string downloadUrl = $"https://github.com/bernarddesfosse/onix_compatible_appx/releases/download/{version}/{version}.appx";

            string fileName = $"Minecraft-{version}.zip";
            string folderPath = ConfigManager.GetMinecraftInstallationPath();
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            using WebClient client = new();
            client.DownloadProgressChanged += (s, e) =>
            {
                progressBar.Value = e.ProgressPercentage;
            };

            await client.DownloadFileTaskAsync(downloadUrl, filePath);

            progressBar.Visibility = Visibility.Collapsed;

            // Call InstallMinecraftPackage method
            await InstallMinecraftPackage();
        }

        public static void RegisterAppPackage(string packagePath)
        {
            PackageManager packageManager = new();

            var packageUri = new Uri(packagePath);
            var deploymentOperation = packageManager.RegisterPackageAsync(packageUri, null, DeploymentOptions.DevelopmentMode);
            string installationPath = ConfigManager.GetMinecraftInstallationPath();

            deploymentOperation.Completed += (asyncInfo, asyncStatus) =>
            {
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (asyncStatus == AsyncStatus.Completed)
                        {
                            StarZMessageBox.ShowDialog("App package registered successfully! Don't forget to apply your profile! Once done, please restart the launcher.", "Success !", false);
                            InstallStatusText!.Foreground = Brushes.AliceBlue;
                            InstallStatusText.Text = "";
                            ConfigManager.SetMinecraftInstallationPath(installationPath);
                            minecraftInstallationPathTextBlock!.Text = installationPath;
                        }
                        else
                        {
                            var errorText = asyncInfo.ErrorCode.Message;
                            StarZMessageBox.ShowDialog($"Failed to register app package. {errorText}", "Failed !", false);
                            InstallStatusText!.Foreground = Brushes.AliceBlue;
                            InstallStatusText.Text = "";
                        }
                    });
                });
            };
        }

        public static void UnregisterMinecraftPackage()
        {
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

        public static async Task InstallMinecraftPackage()
        {
            if (isRunning) // check if installation is already in progress or not
            {
                return;
            }

            // show warning message box to the user
            bool? result = StarZMessageBox.ShowDialog("This will uninstall your current Microsoft Store Minecraft version. You will be prompted to back-up your com.mojang folder if you continue.", "Warning !");

            if (result == false)
            {
                return; // user clicked cancel, so do nothing
            }

            await ToolsManager.SaveProfile(); // ask the user to save the current profile before uninstalling

            isRunning = true; // set installation in progress flag

            // Unregister Minecraft from Microsoft Store
            Application.Current.Dispatcher.Invoke(() =>
            {
                InstallStatusText!.Foreground = Brushes.Red;
                InstallStatusText.Text = "Unregistering Minecraft...";
            });
            UnregisterMinecraftPackage();

            // show file dialog to the user to select the zip file
            Application.Current.Dispatcher.Invoke(() =>
            {
                InstallStatusText!.Foreground = Brushes.Yellow;
                InstallStatusText.Text = "Waiting for user to select a version to install (.zip)...";
            });

            await Task.Run(() =>
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Zip Files|*.zip",
                    InitialDirectory = ConfigManager.GetMinecraftInstallationPath()
                };
                bool? dialogResult = openFileDialog.ShowDialog();

                if (dialogResult != true)
                {
                    isRunning = false; // reset installation in progress flag
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InstallStatusText!.Foreground = Brushes.AliceBlue;
                        InstallStatusText.Text = "";
                    });
                    return;
                }

                string selectedZipFile = openFileDialog.FileName;
                // Get the Minecraft installation path from the config manager
                string installationPath = ConfigManager.GetMinecraftInstallationPath();

                // Define the path for the StarZ X Minecraft folder
                string starzXMinecraftFolderPath = System.IO.Path.Combine(installationPath, "StarZ X Minecraft");

                // Update the UI to indicate the deletion of any previous installed version
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Red;
                    InstallStatusText.Text = "Deleting any previous installed version...";
                });

                // Check if the StarZ X Minecraft folder exists
                if (Directory.Exists(starzXMinecraftFolderPath))
                {
                    // Delete the existing StarZ X Minecraft folder if it exists
                    Directory.Delete(starzXMinecraftFolderPath, true);
                }

                // Create a new StarZ X Minecraft folder
                Directory.CreateDirectory(starzXMinecraftFolderPath);

                // move the selected zip file to the StarZ X Minecraft folder
                string newZipFileName = System.IO.Path.Combine(starzXMinecraftFolderPath, "StarZXMinecraft.zip");
                File.Move(selectedZipFile, newZipFileName);

                // extract the zip file
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Yellow;
                    InstallStatusText.Text = "Extracting the selected version (.zip)...";
                });
                ZipFile.ExtractToDirectory(newZipFileName, starzXMinecraftFolderPath);

                // delete AppxSignature.p7x so that the game can be installed with developer mode
                var signature = System.IO.Path.Combine(starzXMinecraftFolderPath, "AppxSignature.p7x");
                if (File.Exists(signature)) File.Delete(signature);

                // register the app using sideloading method
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Green;
                    InstallStatusText.Text = "Registering and finalizing Minecraft installation...";
                });
                string manifestPath = System.IO.Path.Combine(starzXMinecraftFolderPath, "AppxManifest.xml");
                RegisterAppPackage(manifestPath);

                // delete the initial renamed zip file if it still exists
                if (File.Exists(newZipFileName))
                {
                    File.Delete(newZipFileName);
                }
            });

            isRunning = false; // reset installation in progress flag
        }
    }
}
