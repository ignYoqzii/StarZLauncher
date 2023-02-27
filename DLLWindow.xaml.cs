using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using StarZLauncher.Classes;
using System.IO;
using System.Windows.Controls;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace StarZLauncher
{
    /// <summary>
    /// Interaction logic for ScriptsWindow.xaml
    /// </summary>
    public partial class DLLWindow
    {
        public static Process? Minecraft;
        public DLLWindow dllWindow;
        public static bool IsMinecraftRunning;
        private const string LatestVersionUrl = "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/latest_version.txt";
        private const string DownloadBaseUrl = "https://github.com/Imrglop/Latite-Releases/releases/download/{0}/Latite.{1}.dll";
        private readonly string DllsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "DLLs");

        public DLLWindow()
        {
            InitializeComponent();
        }

        private void SetBackgroundImage()
        {
            // Get file path
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Background.txt");

            // Check if file exists
            if (File.Exists(filePath))
            {
                // Read file contents (image file name)
                string fileName = File.ReadAllText(filePath).Trim();

                // Create file path for image file
                string imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Images", fileName);

                // Check if image file exists
                if (File.Exists(imagePath))
                {
                    // Load image into image control
                    BitmapImage image = new BitmapImage(new Uri(imagePath));
                    BackgroundImage.Source = image;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackgroundImage();
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public MainWindow mainWindow;
        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Check if the main window is null, and create a new one if it is
            if (settingsWindow == null)
            {
                mainWindow = new MainWindow();

                // Set the WindowStartupLocation property of the new window to Manual
                mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                // Set the Top and Left properties of the new window to the same values as the current window
                mainWindow.Top = this.Top;
                mainWindow.Left = this.Left;

                // Hide the current window when the new window is shown
                mainWindow.ContentRendered += (s, args) =>
                {
                    Hide();
                };
            }

            // Show the main window
            mainWindow.Show();

            // Update the Discord Rich Presence state
            if (MainWindow.IsMinecraftRunning)
                DiscordRichPresenceManager.DiscordClient.UpdateState($"Playing Minecraft");
            else
                DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher");
        }
        public SettingsWindow settingsWindow;
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Check if the settings window is null, and create a new one if it is
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();

                // Set the WindowStartupLocation property of the new window to Manual
                settingsWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                // Set the Top and Left properties of the new window to the same values as the current window
                settingsWindow.Top = this.Top;
                settingsWindow.Left = this.Left;

                // Hide the current window when the new window is shown
                settingsWindow.ContentRendered += (s, args) =>
                {
                    Hide();
                };
            }

            // Show the settings window
            settingsWindow.Show();

            // Update the Discord Rich Presence state
            if (MainWindow.IsMinecraftRunning)
                DiscordRichPresenceManager.DiscordClient.UpdateState($"Playing Minecraft");
            else
                DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher's settings");
        }

        // Download Latite's DLLs and launch Minecraft if checkbox is checked
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button == null)
            {
                return;
            }

            string minecraftVersion = button.Tag.ToString();
            string? latestVersion = GetLatestVersion();
            if (latestVersion == null)
            {
                MessageBox.Show("Failed to retrieve latest version information.");
                return;
            }

            string downloadUrl = string.Format(DownloadBaseUrl, latestVersion, minecraftVersion);

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    byte[] data = webClient.DownloadData(downloadUrl);
                    string fileName = Path.Combine(DllsFolderPath, $"Latite.{minecraftVersion}.dll");
                    Directory.CreateDirectory(DllsFolderPath);
                    File.WriteAllBytes(fileName, data);
                    MessageBox.Show($"Latite's DLL for Minecraft {minecraftVersion} has been downloaded!");

                    // Check if the checkbox is checked
                    if (Checkbox.IsChecked == true)
                    {
                        MainWindow mainWindow= new MainWindow();
                        mainWindow.LaunchButton_OnRightClick(sender, e);
                    }
                }
                catch (WebException ex)
                {
                    MessageBox.Show($"Failed to download Latite's DLL for Minecraft {minecraftVersion}. Error: {ex.Message}");
                }
            }
        }

        private static void IfMinecraftExited(object sender, EventArgs e)
        {
            DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher");
            IsMinecraftRunning = false;
        }

        private string? GetLatestVersion()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string latestVersion = webClient.DownloadString(LatestVersionUrl).Trim();
                    return latestVersion;
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Failed to retrieve latest version information. Error: {ex.Message}");
                    return null;
                }
            }
        }
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Onix_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/onixclient");

        private void Latite_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/latite");

        private void Luconia_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/luconia");
    }
}

