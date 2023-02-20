using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DiscordRPC;
using StarZLauncher.Utils;

namespace StarZLauncher
{
    /// <summary>
    /// Interaction logic for CreditWindow.xaml
    /// </summary>
    public partial class CreditWindow
    {
        private static readonly MainWindow MainWindow = new();
        private static readonly SettingsWindow SettingsWindow = new();
        public CreditWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            SettingsWindow.Show();
            if (MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
            else DiscordPresence.DiscordClient.UpdateState("In the launcher's settings");
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            MainWindow.Show();
            if (MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
            else DiscordPresence.DiscordClient.UpdateState("Idling in the launcher");
        }

        private void WindowToolbar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/ScR9MGbRSY");

        private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

        private void DownloadIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");
    }
}
