using System;
using System.Windows;
using System.Windows.Media.Animation;
using StarZLauncher.Classes.Settings;


namespace StarZLauncher.Windows
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
            ThemesManager.CheckForTheme();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(2)));
            animation.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(Window.OpacityProperty, animation);
        }
    }
}
