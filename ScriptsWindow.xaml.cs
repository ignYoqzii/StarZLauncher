using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StarZLauncher.Classes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace StarZLauncher
{
    public partial class ScriptsWindow
    {
        public ScriptsWindow scriptsWindow;
        private string starzScriptsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher\StarZ Scripts\";
        private string resourcePacksFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs\";

        public ScriptsWindow()
        {
            InitializeComponent();
            InitializeDragDrop();
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


        private void GetScriptsButton_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://github.com/bernarddesfosse/OnixClient_Scripts");

        //Open the resourcepacks folder
        private void TexturePackButton_OnLeftClick(object sender, RoutedEventArgs e)
        {
            string minecraftFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs";

            if (Directory.Exists(minecraftFolderPath))
            {
                Process.Start(minecraftFolderPath);
            }
            else
            {
                MessageBox.Show("Error while opening a folder ; is Minecraft installed?");
            }
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
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //                                            //
        //This is the Texture Packs / Scripts Section //
        //                                            //
        private void InitializeDragDrop()
        {
            DragZone.AllowDrop = true;
            DragZone.DragEnter += DragZone_DragEnter;
            DragZone.DragLeave += DragZone_DragLeave;
            DragZone.Drop += DragZone_Drop;
        }

        private void DragZone_DragEnter(object sender, DragEventArgs e)
        {
            if (IsLuaFile(e) || IsMcpackOrZipFile(e))
            {
                DragZone.BorderThickness = new Thickness(2);
                DragZone.BorderBrush = Brushes.White;
            }
        }

        private void DragZone_DragLeave(object sender, DragEventArgs e)
        {
            DragZone.BorderThickness = new Thickness(0.5);
            DragZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
        }

        private void DragZone_Drop(object sender, DragEventArgs e)
        {
            if (IsLuaFile(e))
            {
                MoveLuaFile(e);
            }
            else if (IsMcpackOrZipFile(e))
            {
                ExtractMcpackOrZipFile(e);
            }

            DragZone.BorderThickness = new Thickness(0.5);
            DragZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
        }

        private bool IsLuaFile(DragEventArgs e)
        {
            return e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".lua");
        }

        private bool IsMcpackOrZipFile(DragEventArgs e)
        {
            return e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   (IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".mcpack") ||
                    IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".zip"));
        }

        private bool IsFileOfType(string filePath, string extension)
        {
            return Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase);
        }

        private void MoveLuaFile(DragEventArgs e)
        {
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (!Directory.Exists(starzScriptsFolder))
            {
                Directory.CreateDirectory(starzScriptsFolder);
            }
            File.Move(filePath, starzScriptsFolder + Path.GetFileName(filePath));
        }

        private void ExtractMcpackOrZipFile(DragEventArgs e)
        {
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            string extractPath = resourcePacksFolder + Path.GetFileNameWithoutExtension(filePath);

            try
            {
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }

                if (IsMcpackFile(filePath))
                {
                    ZipFile.ExtractToDirectory(filePath, extractPath);
                }
                else if (IsZipFile(filePath))
                {
                    ZipFile.ExtractToDirectory(filePath, extractPath);
                }

                // Delete the file after extraction is complete
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error extracting file: " + ex.Message);
            }
        }

        private bool IsMcpackFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".mcpack", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsZipFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
        }
    }
}