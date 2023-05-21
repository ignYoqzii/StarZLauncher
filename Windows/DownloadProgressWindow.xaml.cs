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
    }
}
