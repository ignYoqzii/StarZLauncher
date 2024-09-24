using System.Windows;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Windows
{
    public partial class RenameWindow
    {
        public string? CurrentName { get; private set; }
        public string? NewNameDLLs { get; private set; }
        public string? NewNameProfile { get; private set; }

        public RenameWindow(string currentName)
        {
            InitializeComponent();
            CurrentName = currentName;
            DataContext = this;
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
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewNameProfile = NewNameTextBox.Text.Trim();
            NewNameDLLs = NewNameProfile + ".dll";
            DialogResult = true;
            Close();
        }
    }
}

