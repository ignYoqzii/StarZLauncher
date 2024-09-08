using StarZLauncher.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WK.Libraries.BetterFolderBrowserNS;

namespace StarZLauncher.Windows
{
    public partial class MainWindow : Window
    {

        public static MainWindow? mainWindow;
        private static bool isVersionsListLoaded;

        // Making them public to be used somewhere else
        public static Image? HomeTabImage;
        public static Image? ModulesTabImage;
        public static Image? ToolsTabImage;
        public static Image? MusicPlayerTabImage;
        public static Image? SettingsTabImage;
        public static Image? StarZLogoImage;
        public static Image? profilePictureImage;
        public static Image? ComputerTabImage;
        public static Image? CloseImage;
        public static Image? MinimizeImage;
        public static Image? CurrentlyPlayingSongImage;

        public static TabControl? MainTabControl;
        public static TabItem? HometabItem;
        public static TabItem? ModulestabItem;
        public static TabItem? ToolstabItem;
        public static TabItem? MusicPlayertabItem;
        public static TabItem? SettingstabItem;
        public static TabItem? ComputertabItem;

        public static Border? WindowBackground;
        public static Border? DragAndDropZone;
        public static Border? BackgroundForWindowsOnTop;

        public static CheckBox? DarkModeCheckBox;
        public static CheckBox? LightModeCheckBox;

        public static TextBlock? MusicPlayerInformationTextBlock;
        public static TextBlock? discordUsernameTextBlock;
        public static TextBlock? minecraftInstallationPathTextBlock;
        public static TextBlock? cpuTempTextBlock;
        public static TextBlock? gpuTempTextBlock;
        public static TextBlock? cpuNameTextBlock;
        public static TextBlock? gpuNameTextBlock;
        public static TextBlock? cpuLoadTextBlock;
        public static TextBlock? gpuLoadTextBlock;
        public static TextBlock? cpufanTextBlock;
        public static TextBlock? gpufanTextBlock;
        public static TextBlock? memoryTextBlock;
        public static TextBlock? motherboardTextBlock;
        public static TextBlock? ipaddressTextBlock;
        public static TextBlock? InstallStatusText;
        public static TextBlock? CurrentlyPlayingSongTitle;
        public static TextBlock? CurrentlyPlayingSongArtist;
        public static TextBlock? CurrentlyPlayingSongTime;

        public static ProgressBar? CurrentlyPlayingSongProgress;

        public static TextBox? URLTextBox;
        public static TextBox? VideoURLTextBox;
        public static TextBox? injectiondelayTextBox;
        public static TextBox? altTextBox;

        public static ListBox? DLLsListManager;

        public static Label? DefaultDLLText;
        public static Label? CurrentMinecraftVersion;
        public static Label? CurrentLauncherVersion;

        public static StackPanel? FullVersionsListStackPanel;

        private HardwareMonitor? hardwareMonitor;

        public MainWindow()
        {
            InitializeComponent();

            // Images
            HomeTabImage = HomeTab;
            ModulesTabImage = ModulesTab;
            ToolsTabImage = ToolsTab;
            MusicPlayerTabImage = MusicPlayerTab;
            SettingsTabImage = SettingsTab;
            StarZLogoImage = StarZLogo;
            ComputerTabImage = ComputerTab;
            CurrentlyPlayingSongImage = SongImage;

            // TabControl and Items
            MainTabControl = SideBarTabControl;
            HometabItem = HomeTabItem;
            ModulestabItem = ModulesTabItem;
            ToolstabItem = ToolsTabItem;
            MusicPlayertabItem = MusicPlayerTabItem;
            SettingstabItem = SettingsTabItem;
            ComputertabItem = ComputerTabItem;

            // Window background
            DragAndDropZone = DragZone;

            // Themes checkboxes
            DarkModeCheckBox = CheckBoxDarkMode;
            LightModeCheckBox = CheckBoxLightMode;

            // Individual TextBlocks
            MusicPlayerInformationTextBlock = MusicPlayerInfoTextBlock;
            minecraftInstallationPathTextBlock = MinecraftInstallationPathTextBlock;
            cpuTempTextBlock = CPUTempTextBlock;
            gpuTempTextBlock = GPUTempTextBlock;
            gpuNameTextBlock = GPUNameTextBlock;
            cpuNameTextBlock = CPUNameTextBlock;
            gpuLoadTextBlock = GPULoadTextBlock;
            cpuLoadTextBlock = CPULoadTextBlock;
            cpufanTextBlock = CPUFanTextBlock;
            gpufanTextBlock = GPUFanTextBlock;
            memoryTextBlock = MemoryTextBlock;
            motherboardTextBlock = MotherboardTextBlock;
            ipaddressTextBlock = IPAddressTextBlock;
            CurrentlyPlayingSongTitle = CurrentlyPlayingTitle;
            CurrentlyPlayingSongArtist = CurrentlyPlayingArtist;
            CurrentlyPlayingSongTime = CurrentlyPlayingTime;

            // Progress Bar
            CurrentlyPlayingSongProgress = CurrentlyPlayingProgress;

            // Buttons
            CloseImage = CloseButton;
            MinimizeImage = MinimizeButton;

            // TextBoxes
            URLTextBox = urlTextBox;
            injectiondelayTextBox = InjectionDelayTextBox;
            altTextBox = ALTTextBox;

            // DLLs List Box
            DLLsListManager = DllList;

            BackgroundForWindowsOnTop = DarkBackgroundWindows;

            DefaultDLLText = DefaultDLLLabel;

            CurrentMinecraftVersion = CurrentVersion;
            CurrentLauncherVersion = LabelVersion;

            FullVersionsListStackPanel = versionsStackPanel;

            InstallStatusText = InstallationStatus;

            // Loading part
            LoadSettingsFromFile();
            Loader.Load();
            // Start the HardwareMonitoring
            bool debug = ConfigManager.GetDebugHardwareMonitoring();
            if (debug == false)
            {
                hardwareMonitor = new HardwareMonitor();
                Task.Run(() => hardwareMonitor.StartMonitoring());
                HardwareMonitorStatusTextBlock.Visibility = Visibility.Collapsed;
                AboutMenu.Visibility = Visibility.Visible;
            }
            else
            {
                HardwareMonitorStatusTextBlock.Visibility = Visibility.Visible;
                AboutMenu.Visibility = Visibility.Collapsed;
                LogManager.Log("Hardware Monitoring is disabled. To re-enable it, change 'DebugHardwareMonitoring' value in Settings.txt to 'False'.", "HardwareMonitor.txt");
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
        }

        // Animation on program's launch

        private bool isFirstTimeOpened = true;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstTimeOpened)
            {
                DoubleAnimation animation = new(0, 1, new Duration(TimeSpan.FromSeconds(1)))
                {
                    EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                };
                this.BeginAnimation(Window.OpacityProperty, animation);

                // Wait for 3 seconds before setting the visibility of the grid to Hidden
                await Task.Delay(1000);

                DoubleAnimation opacityAnimation = new()
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                };
                OpeningAnim.BeginAnimation(Image.OpacityProperty, opacityAnimation);

                await Task.Delay(500);
                // Handle the Completed event to hide the grid when the opacity animation is finished
                OpeningAnim.Visibility = Visibility.Collapsed;

                isFirstTimeOpened = false;
            }
        }

        private void SideBarTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemesManager.TabItemsSelectionChanged();
        }

        // move the window on screen
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Settings section
        private void CheckBoxLightMode_Checked(object sender, RoutedEventArgs e)
        {
            ThemesManager.LightModeChecked();
        }

        private void CheckBoxDarkMode_Checked(object sender, RoutedEventArgs e)
        {
            ThemesManager.DarkModeChecked();
        }

        private void AddMusic_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.AddMusic();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Image playButton = (Image)sender;
            MusicItem musicItem = (MusicItem)playButton.DataContext;
            string filePath = musicItem.FilePath;
            MusicPlayer.PlayMusic(filePath);
        }

        private void PlayARandomSong_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.ShuffleAndPlayNext();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PauseMusic();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.StopMusic();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Image deleteButton = (Image)sender;
            MusicItem musicItem = (MusicItem)deleteButton.DataContext;
            string filePath = musicItem.FilePath;

            // Delete the file
            File.Delete(filePath);

            // Remove the item from the MusicItems list
            MusicPlayer.DeleteMusic(musicItem);
        }

        private async void urlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string videoUrl = URLTextBox!.Text;
                URLTextBox!.Clear();
                MusicPlayerInformationTextBlock!.Text = "Your music file is being downloaded. This may take a while...";
                await MusicPlayer.DownloadMusicAsync(videoUrl);
            }
        }
        private void JoinDiscordServer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/ScR9MGbRSY");
        }

        // close the program
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // minimize the program
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Only run Minecraft without DLLs or with the default dll
        public void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
        {
            Launch.LaunchGameOnLeftClick();
        }


        //Run Minecraft with a DLL selected from the file explorer window
        public void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
        {
            Launch.LaunchGameOnRightClick();
        }

        // To edit the name of a dll
        private void Edit_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DLLsManager.Edit();
        }

        // set the selected dll as default
        private void SetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DLLsManager.SetDefaultDLL();
        }

        // remove the default dll
        private void ResetSetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DLLsManager.Reset();
        }

        // delete the selected dll
        private void Delete_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DLLsManager.Delete();
        }

        private void AddDllsButton_OnLeftClick(object sender, RoutedEventArgs e)
        {
            DLLsManager.Add();
        }

        private void ShowFullVersionsList_Click(object sender, RoutedEventArgs e)
        {
            bool debug = ConfigManager.GetOfflineMode();
            if (isVersionsListLoaded == false & debug == false)
            {
                MinecraftVersionsListManager.LoadVersionsManager();
                isVersionsListLoaded = true;
            }
            MainMenu.Visibility = Visibility.Collapsed;
            FullVersionsList.Visibility = Visibility.Visible;
        }

        private void GoBackFullVersionList_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            FullVersionsList.Visibility = Visibility.Collapsed;
            MainMenu.Visibility = Visibility.Visible;
        }

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
                StarZMessageBox.ShowDialog("Is Minecraft installed?", "Error !", false);
            }
        }

        // persona skin pack installer if StarZ X Minecraft is installed
        private void Persona_Click(object sender, RoutedEventArgs e)
        {
            ToolsManager.CosmeticsSkinPackApply();
        }

        // shader materials.bin installer if StarZ X Minecraft is installed
        private async void ShaderInstall_Click(object sender, RoutedEventArgs e)
        {
            await ToolsManager.ShaderApply();
        }

        // remove any installed shader
        private void ShaderRemove_Click(object sender, RoutedEventArgs e)
        {
            ToolsManager.ShaderRemove();
        }

        private async void ProfileSave_Click(object sender, RoutedEventArgs e)
        {
            await ToolsManager.SaveProfile();
        }

        private async void ProfileApply_Click(object sender, RoutedEventArgs e)
        {
            await ToolsManager.ApplyProfile();
        }

        // Event for Discord RPC option
        private void DRP_Click(object sender, RoutedEventArgs e)
        {
            if (DRP.IsChecked == true)
            {
                DiscordOptions.Visibility = Visibility.Visible;
                ConfigManager.SetDiscordRPC(true); // set DiscordRPC to true
                if (!DiscordRichPresenceManager.DiscordClient.IsInitialized)
                {
                    DiscordRichPresenceManager.DiscordClient.Initialize();
                }
                DiscordRichPresenceManager.SetPresence();
            }
            else
            {
                DiscordOptions.Visibility = Visibility.Collapsed;
                ConfigManager.SetDiscordRPC(false); // set DiscordRPC to false
                DiscordRichPresenceManager.DiscordClient.ClearPresence();
            }
        }

        // Event for show game version option
        private void SGV_Click(object sender, RoutedEventArgs e)
        {
            if (SGV.IsChecked == true)
            {
                ConfigManager.SetDiscordRPCShowGameVersion(true);
            }
            else
            {
                ConfigManager.SetDiscordRPCShowGameVersion(false);
            }
        }

        // Event for show dll name option
        private void SDN_Click(object sender, RoutedEventArgs e)
        {
            if (SDN.IsChecked == true)
            {
                ConfigManager.SetDiscordRPCShowDLLName(true);
            }
            else
            {
                ConfigManager.SetDiscordRPCShowDLLName(false);
            }
        }

        // Event for launch option

        private void InjectionDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the text value from the TextBox
            string textValue = InjectionDelayTextBox.Text;

            // Validate the entered value
            if (!string.IsNullOrEmpty(textValue))
            {
                // Ensure the entered value is numeric
                if (!int.TryParse(textValue, out int value))
                {
                    // If it's not numeric, remove the last character
                    InjectionDelayTextBox.Text = textValue.Substring(0, textValue.Length - 1);
                }
                else
                {
                    // Ensure the entered value is within the range 0 to 5000
                    if (value < 0 || value > 10000)
                    {
                        // If it's out of range, set it to the nearest valid value
                        InjectionDelayTextBox.Text = (value < 0 ? 0 : 10000).ToString();
                    }
                    else
                    {
                        // Limit the value to 5000 and prevent further insertion
                        if (value == 10000 && textValue.Length > 5)
                        {
                            InjectionDelayTextBox.Text = "10000";
                        }
                        else
                        {
                            // Call the method in ConfigManager to set the injection delay
                            ConfigManager.SetInjectionDelay(textValue);
                        }
                    }
                }
            }
            else
            {
                // Set the value to 0 if the TextBox is empty
                ConfigManager.SetInjectionDelay("0");
            }
        }

        private void ALTTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the text value from the TextBox
            string textValue = altTextBox!.Text;

            // Validate the entered value
            if (!string.IsNullOrEmpty(textValue))
            {
                // Ensure the entered value is numeric
                if (!int.TryParse(textValue, out int value))
                {
                    // If it's not numeric, remove the last character
                    altTextBox!.Text = textValue.Substring(0, textValue.Length - 1);
                }
                else
                {
                    // Ensure the entered value is within the range 0 to 5000
                    if (value < 0 || value > 10000)
                    {
                        // If it's out of range, set it to the nearest valid value
                        altTextBox!.Text = (value < 0 ? 0 : 10000).ToString();
                    }
                    else
                    {
                        // Limit the value to 5000 and prevent further insertion
                        if (value == 10000 && textValue.Length > 5)
                        {
                            altTextBox!.Text = "10000";
                        }
                        else
                        {
                            // Call the method in ConfigManager to set the injection delay
                            ConfigManager.SetAccelerateLoadingTime(textValue);
                        }
                    }
                }
            }
            else
            {
                // Set the value to 0 if the TextBox is empty
                ConfigManager.SetAccelerateLoadingTime("0");
            }
        }

        // Load the default values of the checkboxes, drop downs... on window's loading
        private void LoadSettingsFromFile()
        {
            bool debug = ConfigManager.GetOfflineMode();
            if (debug == true)
            {
                OfflineModeToggle.IsChecked = true;
            }
            else
            {
                OfflineModeToggle.IsChecked = false;
            }
            bool DiscordRPCisChecked = ConfigManager.GetDiscordRPC();
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
            bool DiscordRPCSGVisChecked = ConfigManager.GetDiscordRPCShowGameVersion();
            if (DiscordRPCSGVisChecked == true)
            {
                SGV.IsChecked = true;
            }
            else
            {
                SGV.IsChecked = false;
            }
            bool DiscordRPCSDNisChecked = ConfigManager.GetDiscordRPCShowDLLName();
            if (DiscordRPCSDNisChecked == true)
            {
                SDN.IsChecked = true;
            }
            else
            {
                SDN.IsChecked = false;
            }

            string InjectionDelayValue = ConfigManager.GetInjectionDelay();
            InjectionDelayTextBox.Text = InjectionDelayValue;

            string AccelerateLoadingTimeValue = ConfigManager.GetAccelerateLoadingTime();
            altTextBox!.Text = AccelerateLoadingTimeValue;

            string MinecraftInstallationPath = ConfigManager.GetMinecraftInstallationPath();
            MinecraftInstallationPathTextBlock!.Text = MinecraftInstallationPath;
        }

        private void VSyncEnable_Click(object sender, RoutedEventArgs e)
        {
            ToolsManager.VSyncEnable();
        }

        private void VSyncDisable_Click(object sender, RoutedEventArgs e)
        {
            ToolsManager.VSyncDisable();
        }

        private void InjectionDelayInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StarZMessageBox.ShowDialog("Reads a value between 0 and 10000 representing time in milliseconds before a DLL is injected into the game. 0 means no delay and 10000 means 10 seconds. Useful for those whose game would crash when using StarZ.", "Injection Delay", false);
        }

        private void AccelerateLoadingTimeInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StarZMessageBox.ShowDialog("Reads a value between 0 and 10000 representing time in milliseconds before the RuntimeBroker.exe process is killed to allow the game to load faster. 0 means it won't take effects (disabled).", "Accelarate Loading Time", false);
        }

        private void LauncherFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher");

            if (Directory.Exists(folderPath))
            {
                // Open the folder
                Process.Start(folderPath);
            }
            else
            {
                StarZMessageBox.ShowDialog("The launcher's folder was deleted. Please close the launcher and reopen it.", "Error !", false); // If this happens, the launcher would'nt even work.
            }
        }

        private void MinecraftInstallationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BetterFolderBrowser();
            {
                dialog.Title = "Select the Minecraft installation folder (.exe folder)";
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sourcePath = dialog.SelectedFolder;
                ConfigManager.SetMinecraftInstallationPath(sourcePath);
                MinecraftInstallationPathTextBlock.Text = sourcePath;
            }

        }

        private void MinecraftInstallationPathReset_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Versions");

            // Create the directory if it does not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            ConfigManager.SetMinecraftInstallationPath(path);
            MinecraftInstallationPathTextBlock.Text = path;
        }

        private void OfflineMode_Click(object sender, RoutedEventArgs e)
        {
            if (OfflineModeToggle.IsChecked == true)
            {
                ConfigManager.SetOfflineMode(true);
            }
            else
            {
                ConfigManager.SetOfflineMode(false);
            }
        }

        private void MusicScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double scrollAmount = e.Delta > 0 ? -1 : 1; // Adjust the scroll amount as needed
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
                e.Handled = true; // Mark the event as handled
            }
        }
    }
}
