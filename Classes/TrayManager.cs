using System.Drawing;
using System.Windows.Forms;

namespace StarZLauncher
{
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


