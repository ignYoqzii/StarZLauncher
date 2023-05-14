using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using StarZLauncher.Classes;
using StarZLauncher.Classes.Settings;
using StarZLauncher.Classes.Discord;


namespace StarZLauncher.Windows
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Loader.LoadBackgroundImage();
            LoadSettingsFromFile();
            ThemesManager.CheckForTheme();
        }

        // Change the background image on the click of the button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create directory for images if it doesn't exist
            string imagesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Images");
            Directory.CreateDirectory(imagesDir);

            // Prompt user to select image file
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string newImageName = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(openFileDialog.FileName);
                string newImagePath = Path.Combine(imagesDir, newImageName);

                // Check if file with same name already exists in Images directory
                if (File.Exists(newImagePath))
                {
                    MessageBoxResult result = MessageBox.Show("A file with the same name already exists. Do you want to replace the existing file?", "File exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                // Copy new image to Images directory with unique name
                File.Copy(openFileDialog.FileName, newImagePath, true);

                ConfigTool.SetLauncherBackground(newImageName);

                // Load new image
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(newImagePath);
                bitmapImage.EndInit();
                BackgroundImage.Source = bitmapImage;
                Loader.LoadBackgroundImage();
            }
        }

        // Load the default values of the checkboxes, drop downs... on window's loading
        private void LoadSettingsFromFile()
        {
            bool DiscordRPCisChecked = ConfigTool.GetDiscordRPC();
            if (DiscordRPCisChecked == true)
            {
                DRP.IsChecked = true;
                DiscordOptions.Visibility = Visibility.Visible;
            }
            else
            {
                DRP.IsChecked = false;
                DiscordOptions.Visibility = Visibility.Collapsed;
            }
            bool DiscordRPCSGVisChecked = ConfigTool.GetDiscordRPCShowGameVersion();
            if (DiscordRPCSGVisChecked == true)
            {
                SGV.IsChecked = true;
            }
            else
            {
                SGV.IsChecked = false;
            }
            bool DiscordRPCSDNisChecked = ConfigTool.GetDiscordRPCShowDLLName();
            if (DiscordRPCSDNisChecked == true)
            {
                SDN.IsChecked = true;
            }
            else
            {
                SDN.IsChecked = false;
            }

            string LaunchOptionisSelected = ConfigTool.GetLaunchOption();
            if (LaunchOptionisSelected == "RemainOpen")
            {
                ComboBoxLaunchOption.SelectedIndex = 0;
            }
            if (LaunchOptionisSelected == "MinimizeToTray")
            {
                ComboBoxLaunchOption.SelectedIndex = 1;
            }
            if (LaunchOptionisSelected == "Minimize")
            {
                ComboBoxLaunchOption.SelectedIndex = 2;
            }

            string LauncherColorisSelected = ConfigTool.GetLauncherColor();
            if (LauncherColorisSelected == "Default")
            {
                ComboBoxLauncherColors.SelectedIndex = 0;
            }
            if (LauncherColorisSelected == "Red")
            {
                ComboBoxLauncherColors.SelectedIndex = 1;
            }
        }

        // Event for Discord RPC option
        private void DRP_Click(object sender, RoutedEventArgs e)
        {
            if (DRP.IsChecked == true)
            {
                DiscordOptions.Visibility = Visibility.Visible;
                ConfigTool.SetDiscordRPC(true); // set DiscordRPC to true
                if (!DiscordRichPresenceManager.DiscordClient.IsInitialized)
                {
                    DiscordRichPresenceManager.DiscordClient.Initialize();
                }
                DiscordRichPresenceManager.SetPresence();
            }
            else
            {
                DiscordOptions.Visibility = Visibility.Collapsed;
                ConfigTool.SetDiscordRPC(false); // set DiscordRPC to false
                DiscordRichPresenceManager.DiscordClient.ClearPresence();
            }
        }

        // Event for show game version option
        private void SGV_Click(object sender, RoutedEventArgs e)
        {
            if (SGV.IsChecked == true)
            {
                ConfigTool.SetDiscordRPCShowGameVersion(true);
            }
            else
            {
                ConfigTool.SetDiscordRPCShowGameVersion(false);
            }
        }

        // Event for show dll name option
        private void SDN_Click(object sender, RoutedEventArgs e)
        {
            if (SDN.IsChecked == true)
            {
                ConfigTool.SetDiscordRPCShowDLLName(true);
            }
            else
            {
                ConfigTool.SetDiscordRPCShowDLLName(false);
            }
        }

        // Event for launch option
        private void ComboBoxLaunchOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = ComboBoxLaunchOption.SelectedItem as ComboBoxItem ?? throw new ArgumentNullException(nameof(ComboBoxLaunchOption.SelectedItem));
            string selectedContent = selectedItem.Content?.ToString() ?? throw new ArgumentNullException(nameof(selectedItem.Content));

            // Do something based on the selected content
            if (selectedContent == "Remain open")
            {
                ConfigTool.SetLaunchOption("RemainOpen");
            }
            else if (selectedContent == "Minimize to tray")
            {
                ConfigTool.SetLaunchOption("MinimizeToTray");
            }
            else if (selectedContent == "Minimize")
            {
                ConfigTool.SetLaunchOption("Minimize");
            }
        }

        // Event for launcher colors / themes
        private void ComboBoxLauncherColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = ComboBoxLauncherColors.SelectedItem as ComboBoxItem ?? throw new ArgumentNullException(nameof(ComboBoxLauncherColors.SelectedItem));
            string selectedContent = selectedItem.Content?.ToString() ?? throw new ArgumentNullException(nameof(selectedItem.Content));

            // Do something based on the selected content
            if (selectedContent == "Default")
            {
                ConfigTool.SetLauncherColor("Default");
                ThemesManager.DefaultButtonsTheme();
            }
            else if (selectedContent == "Red")
            {
                ConfigTool.SetLauncherColor("Red");
                ThemesManager.RedButtonsTheme();
            }
        }

        // Close the settings window
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        // Move the window on screen
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

