using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.DirectoryServices.AccountManagement;
using System.Windows.Media.Animation;
using static StarZLauncher.Classes.MusicPlayer;
using static StarZLauncher.Windows.MainWindow;
using System.Windows.Media;

namespace StarZLauncher.Classes
{

    /// <summary>
    /// A simple class to load some stuff when the program starts.
    /// </summary>
    public static class Loader
    {

        public static void Load()
        {
            string logFileName = $"AppStartup.txt";

            try
            {
                VersionHelper.LoadCurrentVersions();
                LogManager.Log("Loaded the current installed versions of Minecraft and the launcher.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading current versions: {ex.Message}", logFileName);
            }

            try
            {
                DLLsManager.LoadDefaultDLL();
                LogManager.Log("Default DLL loaded and ready for injection.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading default DLL: {ex.Message}", logFileName);
            }

            try
            {
                ToolsManager.InitializeDragDrop();
                LogManager.Log("Initialized drag and drop for the Utilities tab.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error initializing drag and drop: {ex.Message}", logFileName);
            }

            try
            {
                DLLsManager.LoadDLLs();
                LogManager.Log("Loaded stored DLLs.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading DLLs: {ex.Message}", logFileName);
            }

            try
            {
                CheckForThemes();
                LogManager.Log("Theme loaded.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading the theme: {ex.Message}", logFileName);
            }

            try
            {
                LoadMusicFiles();
                LogManager.Log("Stored music files loaded.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading music files: {ex.Message}", logFileName);
            }

            try
            {
                HardwareMonitor.GetLocalIPAddress();
                LogManager.Log("Retrieved local IP address for displaying.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error retrieving local IP address: {ex.Message}", logFileName);
            }
            try
            {
                Task.Run(() => LoadUserProfileInfo());
                LogManager.Log("Loaded Windows account username.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error retrieving Windows account username: {ex.Message}", logFileName);
            }

            bool debug = ConfigManager.GetOfflineMode();
            if (!debug)
            {
                try
                {
                    VersionHelper.CheckForUpdates();
                    LogManager.Log("Checked for updates.", logFileName);
                }
                catch (Exception ex)
                {
                    LogManager.Log($"Error checking for updates: {ex.Message}", logFileName);
                }
            }
            else
            {
                LogManager.Log("Offline mode is enabled. Skipping update check.", logFileName);
            }
        }

        public static void CheckForThemes()
        {
            string Mode = ConfigManager.GetTheme();
            if (Mode == "Light")
            {
                ThemesManager.LoadTheme("LightMode.xaml");
                LightModeCheckBox!.IsChecked = true;
                LightModeCheckBox!.IsEnabled = false;
            }
            else if (Mode == "Dark")
            {
                ThemesManager.LoadTheme("DarkMode.xaml");
                DarkModeCheckBox!.IsChecked = true;
                DarkModeCheckBox!.IsEnabled = false;
            }
        }

        public static void LoadMusicFiles()
        {
            MusicItems.Clear(); // Refreshes the list in case

            string[] musicFiles = Directory.GetFiles(MusicDirectoryPath, "*.mp3");

            foreach (string filePath in musicFiles)
            {
                MusicItem musicItem = new(filePath);
                MusicItems.Add(musicItem);
            }
            CheckForItemsCount();
        }

        private static void CheckForItemsCount()
        {
            if (MusicItems.Count == 0)
            {
                MusicPlayerInformationTextBlock!.Text = "There's nothing here. Add musics to get started!";
                MusicPlayerInformationTextBlock!.Visibility = Visibility.Visible;
            }
            else
            {
                MusicPlayerInformationTextBlock!.Visibility = Visibility.Collapsed;
            }
        }

        public static async Task LoadUserProfileInfo()
        {
            // Get the current greeting based on the time of day
            string greeting = GetGreetingBasedOnTime();

            // Set the initial text to "Greeting" and configure opacity
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                WelcomeBack!.Text = greeting;
                WelcomeBack.Opacity = 0; // Start with invisible text

                // Create a fade-in animation for the initial text
                DoubleAnimation fadeInWelcomeAnimation = new()
                {
                    From = 0, // Start from invisible
                    To = 1,   // Fade in to fully visible
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)) // Half a second fade-in
                };

                // Apply the animation to the Opacity property
                WelcomeBack!.BeginAnimation(UIElement.OpacityProperty, fadeInWelcomeAnimation);
            });

            // Load the user display name asynchronously
            string displayName = Environment.UserName; // Default to Environment.UserName
            try
            {
                UserPrincipal userPrincipal = await Task.Run(() => UserPrincipal.Current);
                displayName = userPrincipal.DisplayName;
            }
            catch
            {
                throw new Exception("No username found.");
            }

            // Check if displayName is null or empty
            if (string.IsNullOrWhiteSpace(displayName))
            {
                // Stop execution and do not update the username textblock if displayName is empty or null
                return;
            }

            // Update the text and apply fade-in animation for the complete text
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Update the text to include the username
                WelcomeUsername!.Text = $", {displayName} !";

                // Create a fade-in animation for the updated text
                DoubleAnimation fadeInUsernameAnimation = new()
                {
                    From = 0, // Start from invisible
                    To = 1,   // Fade in to fully visible
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)) // Half a second fade-in
                };

                // Apply the animation to the Opacity property
                WelcomeUsername.BeginAnimation(UIElement.OpacityProperty, fadeInUsernameAnimation);
            });
        }

        public static string GetGreetingBasedOnTime()
        {
            // Get the current hour
            int currentHour = DateTime.Now.Hour;

            // Determine the greeting based on the time of day
            if (currentHour >= 5 && currentHour < 12)
            {
                return "Good morning";
            }
            else if (currentHour >= 12 && currentHour < 17)
            {
                return "Good afternoon";
            }
            else
            {
                return "Good evening";
            }
        }

    }
}
