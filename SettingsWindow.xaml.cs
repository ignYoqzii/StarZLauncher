using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
using System;
using Microsoft.Win32;


namespace StarZLauncher
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create directory for images if it doesn't exist
            string imagesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Images");
            Directory.CreateDirectory(imagesDir);

            // Prompt user to select image file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                string oldImageName = BackgroundImage.Source.ToString().Substring(BackgroundImage.Source.ToString().LastIndexOf("/") + 1);
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

                // Update Background.txt file with new image name
                string backgroundFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Background.txt");
                File.WriteAllText(backgroundFilePath, newImageName);

                // Load new image
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(newImagePath);
                bitmapImage.EndInit();
                BackgroundImage.Source = bitmapImage;
                foreach (System.Windows.Window window in Application.Current.Windows)
                {
                    // Check if the window is a MainWindow or a subclass of MainWindow
                    if (window is MainWindow || window.GetType().IsSubclassOf(typeof(MainWindow)))
                    {
                        // Call the SetBackgroundImage method on the MainWindow instance
                        ((MainWindow)window).SetBackgroundImage();
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

