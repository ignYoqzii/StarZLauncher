using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class ThemesManager
    {
        private static readonly Dictionary<TabItem, (string, string, string, string, string, string)> TabImagePaths = new()
        {
            { HometabItem!, ("/Resources/HomeWhite.png", "/Resources/ModulesGray.png", "/Resources/ToolsGray.png", "/Resources/MusicPlayerGray.png", "/Resources/ComputerGray.png", "/Resources/SettingsGray.png") },
            { ModulestabItem!, ("/Resources/HomeGray.png", "/Resources/ModulesWhite.png", "/Resources/ToolsGray.png", "/Resources/MusicPlayerGray.png", "/Resources/ComputerGray.png", "/Resources/SettingsGray.png") },
            { ToolstabItem!, ("/Resources/HomeGray.png", "/Resources/ModulesGray.png", "/Resources/ToolsWhite.png", "/Resources/MusicPlayerGray.png", "/Resources/ComputerGray.png", "/Resources/SettingsGray.png") },
            { MusicPlayertabItem!, ("/Resources/HomeGray.png", "/Resources/ModulesGray.png", "/Resources/ToolsGray.png", "/Resources/MusicPlayerWhite.png", "/Resources/ComputerGray.png", "/Resources/SettingsGray.png") },
            { ComputertabItem!, ("/Resources/HomeGray.png", "/Resources/ModulesGray.png", "/Resources/ToolsGray.png", "/Resources/MusicPlayerGray.png", "/Resources/ComputerWhite.png", "/Resources/SettingsGray.png") },
            { SettingstabItem!, ("/Resources/HomeGray.png", "/Resources/ModulesGray.png", "/Resources/ToolsGray.png", "/Resources/MusicPlayerGray.png", "/Resources/ComputerGray.png", "/Resources/SettingsWhite.png") }
        };

        public static void TabItemsSelectionChanged()
        {
            if (MainTabControl!.SelectedItem is TabItem selectedTabItem && TabImagePaths.TryGetValue(selectedTabItem, out var imagePaths))
            {
                SetTabImages(imagePaths);
            }
        }

        private static void SetTabImages((string home, string modules, string tools, string musicPlayer, string computer, string settings) imagePaths)
        {
            HomeTabImage!.Source = CreateBitmapImage(imagePaths.home);
            ModulesTabImage!.Source = CreateBitmapImage(imagePaths.modules);
            ToolsTabImage!.Source = CreateBitmapImage(imagePaths.tools);
            MusicPlayerTabImage!.Source = CreateBitmapImage(imagePaths.musicPlayer);
            ComputerTabImage!.Source = CreateBitmapImage(imagePaths.computer);
            SettingsTabImage!.Source = CreateBitmapImage(imagePaths.settings);
        }

        private static BitmapImage CreateBitmapImage(string uriSource)
        {
            return new BitmapImage(new Uri(uriSource, UriKind.Relative));
        }

        public static void LoadTheme(string theme)
        {
            var uri = new Uri($"/Themes/{theme}", UriKind.RelativeOrAbsolute);
            var resourceDict = new ResourceDictionary { Source = uri };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        public static void DarkModeChecked()
        {
            UpdateThemeAndCheckboxes("DarkMode.xaml", "Dark", LightModeCheckBox!, DarkModeCheckBox!);
        }

        public static void LightModeChecked()
        {
            UpdateThemeAndCheckboxes("LightMode.xaml", "Light", DarkModeCheckBox!, LightModeCheckBox!);
        }

        private static void UpdateThemeAndCheckboxes(string themeFileName, string themeName, CheckBox uncheckedCheckBox, CheckBox checkedCheckBox)
        {
            LoadTheme(themeFileName);
            ConfigManager.SetTheme(themeName);

            uncheckedCheckBox.IsChecked = false;
            uncheckedCheckBox.IsEnabled = true;
            checkedCheckBox.IsChecked = true;
            checkedCheckBox.IsEnabled = false;
        }

    }
}