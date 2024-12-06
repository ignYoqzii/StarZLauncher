using System.Windows;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Windows
{
    public partial class EditWindow
    {
        public string? CurrentName { get; private set; }
        public string? NewName { get; private set; }
        public string? CurrentURL { get; private set; }
        public string? NewURL { get; private set; }

        public EditWindow(string currentName, string? currentUrl = null, bool editUrl = false)
        {
            InitializeComponent();
            CurrentName = currentName;
            CurrentURL = currentUrl;
            DataContext = this;
            if (!editUrl)
            {
                NewURLTextBox.IsEnabled = false;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Hide the BackgroundForWindowsOnTop element when closing
            BackgroundForWindowsOnTop!.Visibility = Visibility.Hidden;

            base.OnClosing(e); // Call the base method
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewName = NewNameTextBox.Text.Trim();
            NewURL = NewURLTextBox.Text.Trim();
            DialogResult = true;
        }
    }
}

