using System.Windows;
using System.Windows.Input;
using StarZLauncher.Utils;

namespace StarZLauncher
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow
    {
        public ChangelogWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (!MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState("Idling in the client");
            if (MainWindow.IsMinecraftRunning)
                DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
        }

        private void WindowToolbar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
