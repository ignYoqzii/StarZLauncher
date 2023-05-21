using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace StarZLauncher.Classes.Settings
{
    public static class ThemesManager
    {
        static ThemesManager()
        {
        }

        public static void CheckForTheme()
        {
            if (ConfigTool.GetLauncherColor() == "Red")
            {
                RedButtonsTheme();
            }
        }
        public static void DefaultButtonsTheme()
        {
            // Get all open windows in the application
            foreach (Window window in Application.Current.Windows)
            {
                // Basic buttons
                var basicButtons = new string[] { "LaunchButton", "AddDLLsButton", "GetScriptsButton", "TexturePacksFolderButton", "Cosmetic", "Shader", "ShaderRemove", "LauncherFolderButton", "ChangeImageButton", "CancelDownloadButton", "CancelRenameButton", "SaveRenameButton", "ProfileApply", "ProfileSave" };

                foreach (string buttonName in basicButtons)
                {
                    if (window.FindName(buttonName) is Button button)
                    {
                        button.Template = (ControlTemplate)button.FindResource("DefaultButtons");
                    }
                }

                // Download buttons
                var downloadButtons = new string[] { "downloadButton1_19", "downloadButton1_18", "downloadButton1_17", "downloadButton1_16" };

                foreach (string buttonName in downloadButtons)
                {
                    if (window.FindName(buttonName) is Button button)
                    {
                        button.Style = (Style)button.FindResource("DefaultDownloadButtons");
                    }
                }

                // TabItems
                var tabItems = new string[] { "MainMenuTab", "DLLsManagerTab", "ToolsManagerTab", "AboutPageTab" };

                foreach (string tabName in tabItems)
                {
                    if (window.FindName(tabName) is TabItem tabItem)
                    {
                        tabItem.Style = (Style)tabItem.FindResource("DefaultTabControlStyle");
                    }
                }

                // ComboBoxes
                var comboBoxes = new string[] { "ComboBoxLaunchOption", "ComboBoxLauncherColors" };

                foreach (string comboBoxName in comboBoxes)
                {
                    if (window.FindName(comboBoxName) is ComboBox comboBox)
                    {
                        comboBox.Style = (Style)comboBox.FindResource("DefaultComboboxStyle");
                    }
                }

                // ToggleButtons
                var toggleButtons = new string[] { "DRP", "SGV", "SDN" };

                foreach (string toggleButtonName in toggleButtons)
                {
                    if (window.FindName(toggleButtonName) is ToggleButton toggleButton)
                    {
                        toggleButton.Style = (Style)toggleButton.FindResource("DefaultToggleButtonStyle");
                    }
                }

                // StarZIcon
                var starzIcons = new string[] { "StarZIcon" };

                foreach (string starzIconName in starzIcons)
                {
                    if (window.FindName(starzIconName) is Image starzIcon)
                    {
                        starzIcon.Source = new BitmapImage(new Uri("/Images/starz.png", UriKind.Relative));
                    }
                }
            }
        }

        public static void RedButtonsTheme()
        {
            // Get all open windows in the application
            foreach (Window window in Application.Current.Windows)
            {
                // Basic buttons
                var basicButtons = new string[] { "LaunchButton", "AddDLLsButton", "GetScriptsButton", "TexturePacksFolderButton", "Cosmetic", "Shader", "ShaderRemove", "LauncherFolderButton", "ChangeImageButton", "CancelDownloadButton", "CancelRenameButton", "SaveRenameButton", "ProfileApply", "ProfileSave" };

                foreach (string buttonName in basicButtons)
                {
                    if (window.FindName(buttonName) is Button button)
                    {
                        button.Template = (ControlTemplate)button.FindResource("RedButtons");
                    }
                }

                // Download buttons
                var downloadButtons = new string[] { "downloadButton1_19", "downloadButton1_18", "downloadButton1_17", "downloadButton1_16" };

                foreach (string buttonName in downloadButtons)
                {
                    if (window.FindName(buttonName) is Button button)
                    {
                        button.Style = (Style)button.FindResource("RedDownloadButtons");
                    }
                }

                // TabItems
                var tabItems = new string[] { "MainMenuTab", "DLLsManagerTab", "ToolsManagerTab", "AboutPageTab" };

                foreach (string tabName in tabItems)
                {
                    if (window.FindName(tabName) is TabItem tabItem)
                    {
                        tabItem.Style = (Style)tabItem.FindResource("RedTabControlStyle");
                    }
                }

                // ComboBoxes
                var comboBoxes = new string[] { "ComboBoxLaunchOption", "ComboBoxLauncherColors" };

                foreach (string comboBoxName in comboBoxes)
                {
                    if (window.FindName(comboBoxName) is ComboBox comboBox)
                    {
                        comboBox.Style = (Style)comboBox.FindResource("RedComboboxStyle");
                    }
                }

                // ToggleButtons
                var toggleButtons = new string[] { "DRP", "SGV", "SDN" };

                foreach (string toggleButtonName in toggleButtons)
                {
                    if (window.FindName(toggleButtonName) is ToggleButton toggleButton)
                    {
                        toggleButton.Style = (Style)toggleButton.FindResource("RedToggleButtonStyle");
                    }
                }

                // StarZIcon
                var starzIcons = new string[] { "StarZIcon" };

                foreach (string starzIconName in starzIcons)
                {
                    if (window.FindName(starzIconName) is Image starzIcon)
                    {
                        starzIcon.Source = new BitmapImage(new Uri("/Images/redstarz.png", UriKind.Relative));
                    }
                }
            }
        }
    }
}
