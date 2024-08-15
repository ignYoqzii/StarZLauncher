using System.Windows;

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewNameProfile = NewNameTextBox.Text.Trim();
            NewNameDLLs = NewNameProfile + ".dll";
            DialogResult = true;
        }
    }
}

