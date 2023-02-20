using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using StarZLauncher.Utils;

namespace StarZLauncher
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {

        private static readonly MainWindow MainWindow = new();
        private static readonly CreditWindow CreditWindow = new();
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            MainWindow.Show();
            if (MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
            else DiscordPresence.DiscordClient.UpdateState("Idling in the launcher");
        }

        private void CreditButton_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
            CreditWindow.Show();
            if (MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
            else DiscordPresence.DiscordClient.UpdateState("Reading the launcher's credits");
        }

        private void WindowToolbar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/ScR9MGbRSY");

        private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

        private void DownloadIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");
    }
}

