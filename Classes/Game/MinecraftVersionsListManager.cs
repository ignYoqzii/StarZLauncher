using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using static StarZLauncher.Windows.MainWindow;
using StarZLauncher.Windows;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32;
using Windows.Management.Deployment;
using Windows.Foundation;
using StarZLauncher.Classes.Tabs;

namespace StarZLauncher.Classes.Game
{
    public static class MinecraftVersionsListManager
    {
        private static bool isRunning = false;
        private static readonly Dictionary<Button, string> downloads = new();
        private static bool isDownloading = false;
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

            // Create a grid with two columns
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Create background colors for alternating rows
            Brush background1 = new SolidColorBrush(Color.FromArgb(255, 17, 17, 17));
            Brush background2 = new SolidColorBrush(Color.FromArgb(128, 57, 57, 57));

            // Add versions to the grid
            for (int i = 0; i < versionList.Length; i++)
            {
                string version = versionList[i];

                // Create a rectangle with the background color for this row
                Brush background = (i % 2 == 0) ? background1 : background2;
                Rectangle rectangle = new() { Fill = background };
                Grid.SetColumnSpan(rectangle, 2); // stretch the rectangle across both columns

                // Create a text block with the version number
                TextBlock textBlock = new()
                {
                    Text = version,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.AliceBlue),
                    FontSize = 15,
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
                    Width = 925,
                    Margin= new Thickness(10, 0, 0, 3),
                    HorizontalAlignment= HorizontalAlignment.Left,
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

                // Add the rectangle, text block, button, progressbar to the grid
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
                grid.Children.Add(rectangle);
                grid.Children.Add(textBlock);
                grid.Children.Add(button);
                grid.Children.Add(progressBar);
                Grid.SetRow(rectangle, i);
                Grid.SetColumn(rectangle, 0);
                Grid.SetRow(textBlock, i);
                Grid.SetColumn(textBlock, 0);
                Grid.SetRow(button, i);
                Grid.SetColumn(button, 1);
                Grid.SetRow(progressBar, i);
                Grid.SetColumn(progressBar, 0);
                Grid.SetColumnSpan(progressBar, 2);
            }

            // Add the grid to the stack panel
            FullVersionsListStackPanel?.Children.Add(grid);
        }

        private static async Task DownloadVersionAsync(string version, ProgressBar progressBar)
        {
            string downloadUrl = $"https://github.com/BionicBen/ProjectStarVersionChanger/releases/download/{version}/Minecraft-{version}.Appx";

            string fileName = $"Minecraft-{version}.zip";
            string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions");
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            using WebClient client = new();
            client.DownloadProgressChanged += (s, e) =>
            {
                progressBar.Value = e.ProgressPercentage;
            };

            await client.DownloadFileTaskAsync(downloadUrl, filePath);

            progressBar.Visibility = Visibility.Collapsed;
        }

        public static async void DownloadPopularVersions(object sender)
        {
            var downloadButton = (Button)sender;
            var version = (string)downloadButton.Tag;
            var fileUrl = $"https://github.com/BionicBen/ProjectStarVersionChanger/releases/download/{version}/Minecraft-{version}.Appx";
            var fileName = $"Minecraft-{version}.zip";
            var folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions");

            if (isDownloading || downloads.ContainsValue(fileUrl))
            {
                return; // Do nothing if a download is already in progress or the file is already being downloaded
            }

            try
            {
                isDownloading = true;

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = System.IO.Path.Combine(folderPath, fileName);

                // Check for internet connectivity
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    StarZMessageBox.ShowDialog("No internet connection available!", "Error !", false);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath); // Delete the file if it already exists
                }

                using (var webClient = new WebClient())
                {
                    BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
                    DownloadProgressWindow downloadProgressWindow = new();
                    downloadProgressWindow.Show();

                    webClient.DownloadProgressChanged += (s, e) => downloadProgressWindow.progressBar.Value = e.ProgressPercentage;

                    var downloadTask = webClient.DownloadFileTaskAsync(new Uri(fileUrl), filePath);

                    downloadProgressWindow.CancelDownloadButton.Click += (s, e) =>
                    {
                        webClient.CancelAsync(); // Cancel the download if the cancel button is clicked
                        downloadTask.ContinueWith(t => File.Delete(filePath)); // Delete the file if the download is canceled
                        downloadProgressWindow.Close();
                        BackgroundForWindowsOnTop!.Visibility = Visibility.Collapsed;
                    };

                    await downloadTask;

                    downloadProgressWindow.Close();
                    BackgroundForWindowsOnTop!.Visibility = Visibility.Collapsed;
                }

                downloads.Add(downloadButton, fileUrl);

                WebClient_DownloadFileCompleted(downloadButton);
            }
            catch (Exception)
            {
                StarZMessageBox.ShowDialog("The operation was canceled.", "Canceled !", false);
            }
            finally
            {
                isDownloading = false;
            }
        }

        private static void WebClient_DownloadFileCompleted(Button downloadButton)
        {
            downloads.Remove(downloadButton);

            if (downloads.Count == 0)
            {
                // Show a message box to inform the user that the download has finished
                StarZMessageBox.ShowDialog("Download completed", "Success !", false);
            }
        }

        public static void RegisterAppPackage(string packagePath)
        {
            PackageManager packageManager = new();

            var packageUri = new Uri(packagePath);
            var deploymentOperation = packageManager.RegisterPackageAsync(packageUri, null, DeploymentOptions.DevelopmentMode);

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
                            InstallStatusText2!.Foreground = Brushes.AliceBlue;
                            InstallStatusText2.Text = "";
                        }
                        else
                        {
                            var errorText = asyncInfo.ErrorCode.Message;
                            StarZMessageBox.ShowDialog($"Failed to register app package. {errorText}", "Failed !", false);
                            InstallStatusText!.Foreground = Brushes.AliceBlue;
                            InstallStatusText.Text = "";
                            InstallStatusText2!.Foreground = Brushes.AliceBlue;
                            InstallStatusText2.Text = "";
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

            ToolsManager.SaveProfile(); // ask the user to save the current profile before uninstalling

            isRunning = true; // set installation in progress flag

            // Unregister Minecraft from Microsoft Store
            Application.Current.Dispatcher.Invoke(() =>
            {
                InstallStatusText!.Foreground = Brushes.Red;
                InstallStatusText.Text = "Unregistering Minecraft...";
                InstallStatusText2!.Foreground = Brushes.Red;
                InstallStatusText2.Text = "Unregistering Minecraft...";
            });
            UnregisterMinecraftPackage();

            // show file dialog to the user to select the zip file
            Application.Current.Dispatcher.Invoke(() =>
            {
                InstallStatusText!.Foreground = Brushes.Yellow;
                InstallStatusText.Text = "Waiting for user to select a version to install (.zip)...";
                InstallStatusText2!.Foreground = Brushes.Yellow;
                InstallStatusText2.Text = "Waiting for user to select a version to install (.zip)...";
            });

            await Task.Run(() =>
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Zip Files|*.zip",
                    InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions")
                };
                bool? dialogResult = openFileDialog.ShowDialog();

                if (dialogResult != true)
                {
                    isRunning = false; // reset installation in progress flag
                    return;
                }

                string selectedZipFile = openFileDialog.FileName;
                string extractedFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(selectedZipFile), "StarZ X Minecraft");

                // delete the existing StarZ X Minecraft folder if it exists (so it uninstall any previous StarZ X Minecraft version)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Red;
                    InstallStatusText.Text = "Deleting any previous installed version...";
                    InstallStatusText2!.Foreground = Brushes.Red;
                    InstallStatusText2.Text = "Deleting any previous installed version...";
                });
                if (Directory.Exists(extractedFolderPath))
                {
                    Directory.Delete(extractedFolderPath, true);
                }

                // create a new StarZ X Minecraft folder
                Directory.CreateDirectory(extractedFolderPath);

                // move the selected zip file to the StarZ X Minecraft folder
                string newZipFileName = System.IO.Path.Combine(extractedFolderPath, "StarZXMinecraft.zip");
                File.Move(selectedZipFile, newZipFileName);

                // extract the zip file
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Yellow;
                    InstallStatusText.Text = "Extracting the selected version (.zip)...";
                    InstallStatusText2!.Foreground = Brushes.Yellow;
                    InstallStatusText2.Text = "Extracting the selected version (.zip)...";
                });
                ZipFile.ExtractToDirectory(newZipFileName, extractedFolderPath);

                // delete AppxSignature.p7x so that the game can be installed with developer mode
                var signature = System.IO.Path.Combine(extractedFolderPath, "AppxSignature.p7x");
                if (File.Exists(signature)) File.Delete(signature);

                // register the app using sideloading method
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InstallStatusText!.Foreground = Brushes.Green;
                    InstallStatusText.Text = "Registering and finalizing Minecraft installation...";
                    InstallStatusText2!.Foreground = Brushes.Green;
                    InstallStatusText2.Text = "Registering and finalizing Minecraft installation...";
                });
                string manifestPath = System.IO.Path.Combine(extractedFolderPath, "AppxManifest.xml");
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
