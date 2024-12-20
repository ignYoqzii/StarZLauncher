﻿using StarZLauncher.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class ThemesManager
    {
        private static readonly string themeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Theme", "StarZTheme.szt");
        private static readonly string ActiveColor = "#F6F8FA";
        private static readonly string logFileName = "ThemesManager.txt";

        static ThemesManager()
        {
            if (!File.Exists(themeFilePath))
            {
                CreateDefaultThemes();
            }
        }

        private static void CreateDefaultThemes()
        {
            var defaultTheme = new
            {
                LightTheme = CreateLightThemeColorDictionary(),
                DarkTheme = CreateDarkThemeColorDictionary(),
                CustomTheme = CreateCustomThemeColorDictionary()
            };

            SaveThemesToFile(defaultTheme);
        }

        private static Dictionary<string, string> CreateLightThemeColorDictionary() => new()
        {
            { "AccentColor1", "#FF0044EA" },
            { "AccentColor2", "#FF00C7ED" },
            { "PrimaryBackgroundColor", "#FFCCD0D1" },
            { "SecondaryBackgroundColor", "#FFF6F8FA" },
            { "IconColor", "#FF72767C" },
            { "TextColor", "#FF242C35" }
        };

        private static Dictionary<string, string> CreateDarkThemeColorDictionary() => new()
        {
            { "AccentColor1", "#FF0044EA" },
            { "AccentColor2", "#FF00C7ED" },
            { "PrimaryBackgroundColor", "#FF171D22" },
            { "SecondaryBackgroundColor", "#FF242C35" },
            { "IconColor", "#FFF6F8FA" },
            { "TextColor", "#FFF6F8FA" }
        };

        private static Dictionary<string, string> CreateCustomThemeColorDictionary() => new()
        {
            { "AccentColor1", "" },
            { "AccentColor2", "" },
            { "PrimaryBackgroundColor", "" },
            { "SecondaryBackgroundColor", "" },
            { "IconColor", "" },
            { "TextColor", "" }
        };

        private static void SaveThemesToFile(object themes)
        {
            try
            {
                var json = JsonSerializer.Serialize(themes, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(Path.GetDirectoryName(themeFilePath));
                File.WriteAllText(themeFilePath, json);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error saving themes: {ex.Message}", logFileName);
            }
        }

        public static void UpdateAllTabIcons()
        {
            try
            {
                string theme = ConfigManager.GetTheme();
                var colors = LoadThemeColors(theme);

                // Ensure colors are valid
                if (colors == null) return;

                var inactiveBrush = GetSolidColorBrush(colors["IconColor"]);
                var whiteBrush = GetSolidColorBrush(ActiveColor);

                // Update all tab icons to inactive color
                foreach (var tabImage in new[] { HomeTabImage, ModulesTabImage, ToolsTabImage, MusicPlayerTabImage, ComputerTabImage, SettingsTabImage })
                {
                    tabImage!.Fill = inactiveBrush;
                }

                // Ensure the selected tab item is set to white
                if (MainTabControl!.SelectedItem is TabItem selectedTabItem)
                {
                    SetTabColors(selectedTabItem, whiteBrush);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error updating tab icons: {ex.Message}", logFileName);
            }
        }

        private static void SetTabColors(TabItem selectedTabItem, SolidColorBrush whiteBrush)
        {
            // Set the selected tab icon to white
            if (selectedTabItem == null) return;

            if (selectedTabItem == HometabItem) HomeTabImage!.Fill = whiteBrush;
            else if (selectedTabItem == ModulestabItem) ModulesTabImage!.Fill = whiteBrush;
            else if (selectedTabItem == ToolstabItem) ToolsTabImage!.Fill = whiteBrush;
            else if (selectedTabItem == MusicPlayertabItem) MusicPlayerTabImage!.Fill = whiteBrush;
            else if (selectedTabItem == ComputertabItem) ComputerTabImage!.Fill = whiteBrush;
            else if (selectedTabItem == SettingstabItem) SettingsTabImage!.Fill = whiteBrush;
        }

        private static SolidColorBrush GetSolidColorBrush(string colorKey)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorKey));
        }

        public static void ApplyTheme(string themeName)
        {
            try
            {
                ConfigManager.SetTheme(themeName);
                var colors = LoadThemeColors(themeName);
                if (colors != null)
                {
                    UpdateApplicationResources(colors);
                    UpdateAccentGradientBrush(colors);
                    UpdateAllTabIcons();
                    UpdateCheckBoxes(themeName);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error applying theme: {ex.Message}", logFileName);
                bool? result = StarZMessageBox.ShowDialog("It seems that one or more themes contain invalid values, which has caused an error. Would you like to reset the themes? Click 'OK' to reset, or 'Cancel' to continue using the corrupted theme file.", "Warning!", true);
                if (result == true)
                {
                    ResetThemesToDefault();
                }
            }
        }

        private static void UpdateApplicationResources(Dictionary<string, string> colors)
        {
            foreach (var key in colors.Keys)
            {
                System.Windows.Application.Current.Resources[key] = GetSolidColorBrush(colors[key]);
            }
        }

        private static void UpdateAccentGradientBrush(Dictionary<string, string> colors)
        {
            var accentColor1 = (Color)ColorConverter.ConvertFromString(colors["AccentColor1"]);
            var accentColor2 = (Color)ColorConverter.ConvertFromString(colors["AccentColor2"]);

            var accentGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                SpreadMethod = GradientSpreadMethod.Pad,
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };

            accentGradientBrush.GradientStops.Add(new GradientStop(accentColor1, 0.0));
            accentGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(
                (byte)((accentColor1.A + accentColor2.A) / 2),
                (byte)((accentColor1.R + accentColor2.R) / 2),
                (byte)((accentColor1.G + accentColor2.G) / 2),
                (byte)((accentColor1.B + accentColor2.B) / 2)), 0.5)); // Intermediate Color
            accentGradientBrush.GradientStops.Add(new GradientStop(accentColor2, 1.0));

            System.Windows.Application.Current.Resources["AccentColorGradientBrush"] = accentGradientBrush;
        }

        public static Dictionary<string, string>? LoadThemeColors(string themeName)
        {
            try
            {
                string json = File.ReadAllText(themeFilePath);
                var themes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                return themes != null && themes.TryGetValue(themeName, out var colors) ? colors : null;
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading theme colors: {ex.Message}", logFileName);
                return null;
            }
        }

        public static void UpdateThemeColorsInFile(string themeName, Dictionary<string, string> colors)
        {
            try
            {
                string json = File.ReadAllText(themeFilePath);
                var themes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                if (themes != null)
                {
                    themes[themeName] = colors;
                    SaveThemesToFile(themes);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error updating theme colors: {ex.Message}", logFileName);
            }
        }

        public static void SetAndInitializeThemes()
        {
            string theme = ConfigManager.GetTheme();
            ApplyTheme(theme);
        }

        public static void UpdateCheckBoxes(string theme)
        {
            // Set checkbox states based on the theme
            LightModeCheckBox!.IsChecked = theme == "LightTheme";
            LightModeCheckBox.IsEnabled = theme != "LightTheme";

            DarkModeCheckBox!.IsChecked = theme == "DarkTheme";
            DarkModeCheckBox.IsEnabled = theme != "DarkTheme";

            CustomThemeCheckBox!.IsChecked = theme == "CustomTheme";
            CustomThemeCheckBox.IsEnabled = theme != "CustomTheme";
        }

        public static bool AreColorValuesValid(Dictionary<string, string> colors)
        {
            foreach (var color in colors.Values)
            {
                // Check if the color value is null or empty
                if (string.IsNullOrEmpty(color))
                    return false;

                // Further validate if the color string is a valid color format
                try
                {
                    ColorConverter.ConvertFromString(color);
                }
                catch
                {
                    return false; // Invalid color format
                }
            }
            return true; // All color values are valid
        }

        public static void ResetThemesToDefault()
        {
            try
            {
                CreateDefaultThemes();
                ApplyTheme("LightTheme");
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error resetting themes: {ex.Message}", logFileName);
            }
        }

        public static void ExportTheme(bool showSuccessMessage)
        {
            try
            {
                // Check if the current theme file exists
                if (!File.Exists(themeFilePath))
                {
                    LogManager.Log("No theme file found to backup.", logFileName);
                    return;
                }

                // Show the RenameWindow to ask the user for a new file name
                BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
                var editWindow = new EditWindow("StarZTheme.szt");
                bool? result = editWindow.ShowDialog();

                if (result == true && !string.IsNullOrEmpty(editWindow.NewName!))
                {
                    // Define the backup file path with the new file name and .json extension
                    string backupFilePath = Path.Combine(Path.GetDirectoryName(themeFilePath) ?? string.Empty, editWindow.NewName! + ".szt");

                    // Copy the existing theme file to the backup location
                    File.Copy(themeFilePath, backupFilePath, true); // Use true to overwrite if the file exists

                    if (showSuccessMessage)
                    {
                        StarZMessageBox.ShowDialog($"Current theme exported successfully as backup to {backupFilePath}.", "Success!", false);
                    }
                    LogManager.Log($"Current theme exported successfully as backup to {backupFilePath}.", logFileName);
                }
                else
                {
                    LogManager.Log("Backup operation cancelled by user.", logFileName);
                }
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Error exporting current theme as backup: {ex.Message}", "Error!", false);
                LogManager.Log($"Error exporting current theme as backup: {ex.Message}", logFileName);
            }
        }

        public static void ImportTheme()
        {
            try
            {
                // Show an OpenFileDialog to let the user select the theme file to import
                using OpenFileDialog openFileDialog = new();
                openFileDialog.Filter = "StarZ Theme files (*.szt)|*.szt";
                openFileDialog.Title = "Select a valid StarZ Theme .szt file to import";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    LogManager.Log("Import operation cancelled by user.", logFileName);
                    return; // User canceled the dialog
                }

                // Export the current theme as a backup before importing the new one
                ExportTheme(false);

                // Read the contents of the new theme file
                var importedThemeJson = File.ReadAllText(openFileDialog.FileName);
                var importedThemes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(importedThemeJson);

                // Validate the imported theme's structure and color values
                if (importedThemes == null)
                {
                    LogManager.Log("Imported theme file is invalid or corrupt.", logFileName);
                    return;
                }

                // Overwrite the existing StarZTheme.json with the new theme
                File.WriteAllText(themeFilePath, importedThemeJson);

                LogManager.Log($"Theme imported successfully from {openFileDialog.FileName} and applied.", logFileName);

                // Apply the imported theme right after import
                ApplyTheme("CustomTheme");
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Error importing theme: {ex.Message}", "Error!", false);
                LogManager.Log($"Error importing theme: {ex.Message}", logFileName);
            }
        }
    }
}