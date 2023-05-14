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
using System.Linq;

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
                Rectangle rectangle = new Rectangle() { Fill = background };
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
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1)
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

            using (WebClient client = new())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    progressBar.Value = e.ProgressPercentage;
                };

                await client.DownloadFileTaskAsync(downloadUrl, filePath);

                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        public static async void DownloadCustomVersions(object sender)
        {
            var downloadButton = (Button)sender;
            var version = (string)downloadButton.Tag;
            var fileUrl = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/versionsselectorv2/StarZXMinecraft-{version}.zip";
            var fileName = $"StarZXMinecraft-{version}.zip";
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
                    MessageBox.Show("No internet!");
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath); // Delete the file if it already exists
                }

                using (var webClient = new WebClient())
                {
                    var downloadProgressWindow = new DownloadProgressWindow
                    {
                        Owner = mainWindow // Set the owner of the progress window to be the main window
                    };
                    downloadProgressWindow.Show();

                    webClient.DownloadProgressChanged += (s, e) => downloadProgressWindow.progressBar.Value = e.ProgressPercentage;

                    var downloadTask = webClient.DownloadFileTaskAsync(new Uri(fileUrl), filePath);

                    downloadProgressWindow.CancelDownloadButton.Click += (s, e) =>
                    {
                        webClient.CancelAsync(); // Cancel the download if the cancel button is clicked
                        downloadTask.ContinueWith(t => File.Delete(filePath)); // Delete the file if the download is canceled
                        downloadProgressWindow.Close();
                    };

                    await downloadTask;

                    downloadProgressWindow.Close();
                }

                downloads.Add(downloadButton, fileUrl);

                WebClient_DownloadFileCompleted(downloadButton);
            }
            catch (Exception)
            {
                MessageBox.Show($"Canceled!");
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
                MessageBox.Show("Download completed!");
            }
        }

        public static void InstallMinecraftPackage()
        {
            if (isRunning) // check if installation is already in progress or not
            {
                return;
            }

            // show warning message box to the user
            MessageBoxResult result = MessageBox.Show("Warning: This will uninstall your current Microsoft Store Minecraft version and install a modified version of it. If Minecraft is installed somewhere else on your computer, uninstall it to avoid problems. Backup your files if needed! Also, make sure PowerShell is installed on your computer! Are you sure you want to proceed?", "StarZ X Minecraft", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
            {
                return; // user clicked cancel, so do nothing
            }

            isRunning = true; // set installation in progress flag

            // show file dialog to the user to select the zip file
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Zip Files|*.zip",
                InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions")
            };
            bool? _ = openFileDialog.ShowDialog();

            if (!_ == true)
            {
                isRunning = false; // reset installation in progress flag
                return;
            }

            // unregister Minecraft from Microsoft Store
            string unregistercommand = $"Get-AppxPackage *minecraftUWP* | Remove-AppxPackage";
            ProcessStartInfo psiunregister = new("powershell.exe", unregistercommand)
            {
                UseShellExecute = true,
                Verb = "runas" // Run PowerShell as administrator
            };
            Process.Start(psiunregister).WaitForExit();

            string selectedZipFile = openFileDialog.FileName;
            string extractedFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(selectedZipFile), "StarZ X Minecraft");

            // delete the existing StarZ X Minecraft folder if it exists (so it uninstall any previous StarZ X Minecraft version)
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
            ZipFile.ExtractToDirectory(newZipFileName, extractedFolderPath);

            // register the app using Powershell command
            string manifestPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ` Launcher", "StarZ` Versions", "StarZ` X` Minecraft", "AppxManifest.xml");
            string registercommand = $"Add-AppxPackage -Path {manifestPath} -Register";

            ProcessStartInfo psiregister = new("powershell.exe", registercommand)
            {
                UseShellExecute = true,
                Verb = "runas" // Run PowerShell as administrator
            };
            Process.Start(psiregister).WaitForExit();

            // show success message box to the user
            MessageBox.Show("StarZ X Minecraft has been installed successfully! It is recommended to launch the game without the launcher first to avoid any bugs...", "StarZ X Minecraft", MessageBoxButton.OK, MessageBoxImage.Information);

            // delete the initial renamed zip file if it still exists
            if (File.Exists(newZipFileName))
            {
                File.Delete(newZipFileName);
            }

            isRunning = false; // reset installation in progress flag
        }
    }
}
