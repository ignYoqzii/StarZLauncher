using System;
using static StarZLauncher.Classes.Settings.VersionHelper;
using System.Windows.Media.Imaging;
using System.IO;
using StarZLauncher.Classes.Settings;
using StarZLauncher.Classes.Tabs;
using System.Windows.Controls;
using System.Windows;

namespace StarZLauncher.Classes
{
    public static class Loader
    {
        public static void Load() 
        {
            LoadCurrentVersions();
            LoadBackgroundImage();
            ToolsManager.InitializeDragDrop();
            DLLsManager.LoadDefaultDLL();
            DLLsManager.LoadDLLs();
            CheckForUpdates();
            ThemesManager.CheckForTheme();
        }
        public static void LoadBackgroundImage()
        {
            // Read file contents (image file name)
            string fileName = ConfigTool.GetLauncherBackground();

            // Create file path for image file
            string imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Images", fileName);

            // Check if image file exists
            if (File.Exists(imagePath))
            {
                // Load image into image control
                foreach (Window window in Application.Current.Windows)
                {
                    Image backgroundImage = (Image)window.FindName("BackgroundImage");
                    if (backgroundImage != null)
                    {
                        Uri imageUri = new(imagePath);
                        backgroundImage.Source = new BitmapImage(imageUri);
                    }
                }
            }
        }
    }
}
