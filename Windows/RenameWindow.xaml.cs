﻿using System.Windows;
using StarZLauncher.Classes.Settings;

namespace StarZLauncher.Windows
{
    public partial class RenameWindow
    {
        public string? CurrentName { get; private set; }
        public string? NewName { get; private set; }

        public RenameWindow(string currentName)
        {
            InitializeComponent();
            CurrentName = currentName;
            DataContext = this;
            ThemesManager.CheckForTheme();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewName = NewNameTextBox.Text += ".dll";
            DialogResult = true;
        }
    }
}

