﻿using System.Windows;
using System.Windows.Input;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Windows
{
    /// <summary>
    /// I didn't like the default MessageBox provided by Windows, so I made my own.
    /// </summary>
    public partial class StarZMessageBox
    {
        public StarZMessageBox()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(string message, string title, bool showCancelButton = true)
        {
            BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
            StarZMessageBox messageBox = new();
            messageBox.Message.Text = message;
            messageBox.Title.Text = title;

            if (!showCancelButton)
            {
                messageBox.CancelButton.Visibility = Visibility.Collapsed;
            }

            return messageBox.ShowDialog();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            BackgroundForWindowsOnTop!.Visibility = Visibility.Hidden;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            BackgroundForWindowsOnTop!.Visibility = Visibility.Hidden;
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            BackgroundForWindowsOnTop!.Visibility = Visibility.Hidden;
        }
    }
}