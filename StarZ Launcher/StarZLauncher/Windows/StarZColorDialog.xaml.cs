using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using StarZLauncher.Classes;
using System.Collections.Generic;
using System.Windows.Controls;

namespace StarZLauncher.Windows
{
    public partial class StarZColorDialog
    {
        private readonly Dictionary<string, string>? originalColors;
        private bool colorsChanged = false; // Track whether any color has changed

        public StarZColorDialog()
        {
            InitializeComponent();

            // Load the original colors from the config file
            string theme = ConfigManager.GetTheme();
            originalColors = ThemesManager.LoadThemeColors(theme);

            // Set preview borders' backgrounds
            SetPreviewColorBorders();

            UpdateGradientBrush();
        }

        private void SetPreviewColorBorders()
        {
            AccentColor1Border.Background = GetSolidColorBrush("AccentColor1");
            AccentColor2Border.Background = GetSolidColorBrush("AccentColor2");
            BGColor1Border.Background = GetSolidColorBrush("PrimaryBackgroundColor");
            BGColor2Border.Background = GetSolidColorBrush("SecondaryBackgroundColor");
            TextColorBorder.Background = GetSolidColorBrush("TextColor");
            IconColorBorder.Background = GetSolidColorBrush("IconColor");
        }

        private SolidColorBrush GetSolidColorBrush(string colorKey)
        {
            if (originalColors != null && originalColors.TryGetValue(colorKey, out var colorValue) && ThemesManager.AreColorValuesValid(originalColors))
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorValue));
                }
                catch
                {
                    // Log the invalid color or set a default color if needed
                    return new SolidColorBrush(Colors.Transparent); // Or any fallback color
                }
            }
            return new SolidColorBrush(Colors.Transparent); // Or any fallback color
        }

        private void SelectAccentColor1_Click(object sender, RoutedEventArgs e) => SelectColor("AccentColor1", AccentColor1Border);
        private void SelectAccentColor2_Click(object sender, RoutedEventArgs e) => SelectColor("AccentColor2", AccentColor2Border);
        private void SelectBGColor1_Click(object sender, RoutedEventArgs e) => SelectColor("PrimaryBackgroundColor", BGColor1Border);
        private void SelectBGColor2_Click(object sender, RoutedEventArgs e) => SelectColor("SecondaryBackgroundColor", BGColor2Border);
        private void SelectTextColor_Click(object sender, RoutedEventArgs e) => SelectColor("TextColor", TextColorBorder);
        private void SelectIconColor_Click(object sender, RoutedEventArgs e) => SelectColor("IconColor", IconColorBorder);

        private void SelectColor(string colorKey, Border colorBorder)
        {
            using ColorDialog colorDialog = new();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedColorHex = ColorToHex(colorDialog.Color);

                // Only update if the selected color is different
                if (selectedColorHex != originalColors![colorKey])
                {
                    originalColors[colorKey] = selectedColorHex;
                    colorBorder.Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                    UpdateGradientBrush();
                    colorsChanged = true; // Mark that a change has occurred
                }
                else
                {
                    // If the selected color is the same and CustomTheme is not null/empty, do not mark it as changed
                    colorsChanged = false;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (colorsChanged) // Only update if changes were made
            {
                // Update the values in the config file
                ThemesManager.UpdateThemeColorsInFile("CustomTheme", originalColors!);
                DialogResult = true;
            }
            else
            {
                DialogResult = false; // No changes, set result to false
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateGradientBrush()
        {
            try
            {
                var accentColor1 = GetSolidColorBrush("AccentColor1").Color;
                var accentColor2 = GetSolidColorBrush("AccentColor2").Color;

                // Create a new LinearGradientBrush with the selected colors
                var newGradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1),
                    SpreadMethod = GradientSpreadMethod.Pad,
                    MappingMode = BrushMappingMode.RelativeToBoundingBox
                };

                newGradientBrush.GradientStops.Add(new GradientStop(accentColor1, 0));
                newGradientBrush.GradientStops.Add(new GradientStop(
                    Color.FromArgb(
                        (byte)((accentColor1.A + accentColor2.A) / 2),
                        (byte)((accentColor1.R + accentColor2.R) / 2),
                        (byte)((accentColor1.G + accentColor2.G) / 2),
                        (byte)((accentColor1.B + accentColor2.B) / 2)), 0.5)); // Intermediate Color
                newGradientBrush.GradientStops.Add(new GradientStop(accentColor2, 1));

                ColorResultBorder.Background = newGradientBrush;
            }
            catch
            {
                // Log or handle the gradient brush update failure
                ColorResultBorder.Background = new SolidColorBrush(Colors.Transparent); // Or any fallback color
            }
        }

        // Helper method to convert a Color to hex string
        private string ColorToHex(System.Drawing.Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}