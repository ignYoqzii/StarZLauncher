using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using StarZLauncher.Classes;
using System.Linq;

namespace StarZLauncher;

public partial class MainWindow
{
    public static Process? Minecraft;
    public MainWindow mainWindow;
    public static bool IsMinecraftRunning;
    private readonly string DllsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "DLLs");

    public MainWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    //Only run Minecraft without DLLs
    private void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }
    }

    //Run Minecraft with a DLL
    public async void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true,
            InitialDirectory = DllsFolderPath
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        if (Process.GetProcessesByName("Minecraft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        Minecraft = await Task.Run(() =>
        {
            while (Process.GetProcessesByName("Minecraft.Windows").Length == 0) { }
            return Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();
        });
        if (Minecraft == null) return;

        try
        {
            await Injector.WaitForModules();
            Injector.Inject(openFileDialog.FileName);
            IsMinecraftRunning = true;

            Minecraft.EnableRaisingEvents = true;
            Minecraft.Exited += IfMinecraftExited;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to inject DLL file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher");
        IsMinecraftRunning = false;
    }

    public AboutWindow aboutWindow;

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if the about window is null, and create a new one if it is
        if (aboutWindow == null)
        {
            aboutWindow = new AboutWindow();

            // Set the WindowStartupLocation property of the new window to Manual
            aboutWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // Set the Top and Left properties of the new window to the same values as the current window
            aboutWindow.Top = this.Top;
            aboutWindow.Left = this.Left;

            // Hide the current window when the new window is shown
            aboutWindow.ContentRendered += (s, args) =>
            {
                Hide();
            };
        }

        // Show the about window
        aboutWindow.Show();

        // Update the Discord Rich Presence state
        if (MainWindow.IsMinecraftRunning)
            DiscordRichPresenceManager.DiscordClient.UpdateState($"Playing Minecraft");
        else
            DiscordRichPresenceManager.DiscordClient.UpdateState("Reading the launcher's credits");
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
    public ScriptsWindow scriptsWindow;
    private void ScriptsButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if the scripts window is null, and create a new one if it is
        if (scriptsWindow == null)
        {
            scriptsWindow = new ScriptsWindow();

            // Set the WindowStartupLocation property of the new window to Manual
            scriptsWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // Set the Top and Left properties of the new window to the same values as the current window
            scriptsWindow.Top = this.Top;
            scriptsWindow.Left = this.Left;

            // Hide the current window when the new window is shown
            scriptsWindow.ContentRendered += (s, args) =>
            {
                Hide();
            };
        }

        // Show the scripts window
        scriptsWindow.Show();

        // Update the Discord Rich Presence state
        if (MainWindow.IsMinecraftRunning)
            DiscordRichPresenceManager.DiscordClient.UpdateState($"Playing Minecraft");
        else
            DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher's Scripts / Mods");
    }
    public DLLWindow dllWindow;
    private void DLLButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if the dll window is null, and create a new one if it is
        if (dllWindow == null)
        {
            dllWindow = new DLLWindow();

            // Set the WindowStartupLocation property of the new window to Manual
            dllWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // Set the Top and Left properties of the new window to the same values as the current window
            dllWindow.Top = this.Top;
            dllWindow.Left = this.Left;

            // Hide the current window when the new window is shown
            dllWindow.ContentRendered += (s, args) =>
            {
                Hide();
            };
        }

        // Show the dll window
        dllWindow.Show();

        // Update the Discord Rich Presence state
        if (MainWindow.IsMinecraftRunning)
            DiscordRichPresenceManager.DiscordClient.UpdateState($"Playing Minecraft");
        else
            DiscordRichPresenceManager.DiscordClient.UpdateState("In the launcher's DLLs");
    }


    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/ScR9MGbRSY");

    private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

    private void GithubIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");

    private void TwitchIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.twitch.tv/zl1me");

    private void ShowMoreButton_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://feedback.minecraft.net/hc/en-us/sections/360001186971-Release-Changelogs");

    private bool isFirstTimeOpened = true;

    //Window animation everytime it is shown

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
        if (isFirstTimeOpened)
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(2)));
            animation.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(Window.OpacityProperty, animation);

            isFirstTimeOpened = false;
        }
        SetBackgroundImage();
    }

    private readonly Dictionary<Button, string> downloads = new Dictionary<Button, string>();
    private bool isDownloading = false;

    //To download the appxs
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        var downloadButton = (Button)sender;
        var version = (string)downloadButton.Tag;
        var fileUrl = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/versionsselector/Minecraft-{version}.Appx";
        var fileName = $"Minecraft-{version}.Appx";
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions");

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

            var filePath = Path.Combine(folderPath, fileName);

            // Check for internet connectivity
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                throw new Exception("No internet connectivity");
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Delete the file if it already exists
            }

            using (var webClient = new WebClient())
            {
                var downloadProgressWindow = new DownloadProgressWindow();
                downloadProgressWindow.Owner = this; // Set the owner of the progress window to be the main window
                downloadProgressWindow.Show();

                webClient.DownloadProgressChanged += (s, e) => downloadProgressWindow.progressBar.Value = e.ProgressPercentage;

                var downloadTask = webClient.DownloadFileTaskAsync(new Uri(fileUrl), filePath);

                downloadProgressWindow.CancelButton.Click += (s, e) =>
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

    private void WebClient_DownloadFileCompleted(Button downloadButton)
    {
        downloads.Remove(downloadButton);

        if (downloads.Count == 0)
        {
            // Show a message box to inform the user that the download has finished
            MessageBox.Show("Download completed!");
        }
    }

    //To install the appxs
    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions");

        if (Directory.Exists(folderPath))
        {
            // Open file explorer to select appx file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the initial directory to the "StarZ Versions" folder
            openFileDialog.InitialDirectory = folderPath;

            // Set the file filter to only show appx files
            openFileDialog.Filter = "Appx Files (*.appx)|*.appx";

            // Show the dialog and wait for the user to select a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the path of the selected file
                string filePath = openFileDialog.FileName;

                // Start a new process to run the appx file
                Process.Start(filePath);
            }
        }
        else
        {
            // Open file explorer to select appx file
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the file filter to only show appx files
            openFileDialog.Filter = "Appx Files (*.appx)|*.appx";

            // Show the dialog and wait for the user to select a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the path of the selected file
                string filePath = openFileDialog.FileName;

                // Start a new process to run the appx file
                Process.Start(filePath);
            }
        }
    }
    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}