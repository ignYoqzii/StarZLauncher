using System;
using System.Windows;
using System.Windows.Input;
using StarZLauncher.Classes;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace StarZLauncher
{

    public partial class AboutWindow
    {
        public AboutWindow aboutWindow;
        public AboutWindow()
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

        public MainWindow mainWindow;
        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Check if the main window is null, and create a new one if it is
            if (mainWindow == null)
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
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


    }
}
