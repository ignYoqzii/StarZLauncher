using System.Drawing;
using System.Windows.Forms;
using StarZLauncher.Windows;

namespace StarZLauncher.Classes.Settings
{
    // class to create and handle the system tray icon if the option in settings is selected
    public static class TrayManager
    {
        private static NotifyIcon? _notifyIcon;

        public static void MinimizeToTray()
        {

            // Create a new NotifyIcon and set its properties
            _notifyIcon = new()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "StarZ Launcher",
                Visible = true
            };

            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        public static void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // Handle the MouseClick event here
            if (e.Button == MouseButtons.Left)
            {
                MainWindow window = new();
                window.ShowDialog();
            }
        }
    }
}


