using StarZLauncher.Windows;
using System;
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

public class FontInstaller
{
    private const string FontUrl = "https://github.com/ignYoqzii/StarZLauncher/releases/download/font/Outfit-VariableFont.ttf";
    private const string FontFileName = "Outfit-VariableFont.ttf";
    private static readonly string LogFileName = "FontInstaller.txt";
    private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Logs", LogFileName);
    private static readonly string FontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Font", FontFileName);
    private static readonly string FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Font");

    public static async Task FontInstallation()
    {
        try
        {
            if (!Directory.Exists(FontPath))
            {
                Directory.CreateDirectory(FontPath);
            }

            Log("Font installation process started.", LogFilePath);

            if (IsFontInstalled("Outfit"))
            {
                return;
            }

            Log("Font 'Outfit' not installed. Starting download...", LogFilePath);
            await DownloadFontAsync(FontUrl, FontFilePath);

            Log("Installing font...", LogFilePath);
            InstallFont(FontFilePath);

            Log("Font installed successfully. An application restart will be required for changes to take effect.", LogFilePath);
            Application.Current.Dispatcher.Invoke(() =>
            {
                StarZMessageBox.ShowDialog($"The font used by the launcher was registered successfully. An application restart will be required for changes to take effect. To permanently install it, get it from here: {FontFilePath}.", "Info", false);
            });
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}", LogFilePath);
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task DownloadFontAsync(string url, string destinationPath)
    {
        try
        {
            using (HttpClient client = new())
            {
                Log($"Downloading font from {url}...", LogFilePath);
                byte[] fontData = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(destinationPath, fontData);
                Log($"Font downloaded and saved to {destinationPath}.", LogFilePath);
            }
        }
        catch (Exception ex)
        {
            Log($"Failed to download font: {ex.Message}", LogFilePath);
            throw;
        }
    }

    private static void InstallFont(string fontPath)
    {
        try
        {
            if (!File.Exists(fontPath))
            {
                Log($"Font file not found at {fontPath}.", LogFilePath);
                throw new FileNotFoundException("Font file not found", fontPath);
            }

            // Install font by notifying system
            if (AddFontResource(fontPath))
            {
                Log($"Font '{fontPath}' added to system fonts.", LogFilePath);
                SendMessageTimeout(new IntPtr(0xffff), 0x001D, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);
                Log("System notified about the new font.", LogFilePath);
            }
            else
            {
                Log($"Failed to add font '{fontPath}' to system fonts.", LogFilePath);
                throw new InvalidOperationException("Failed to install font.");
            }
        }
        catch (Exception ex)
        {
            Log($"Failed to install font: {ex.Message}", LogFilePath);
            throw;
        }
    }

    private static bool IsFontInstalled(string fontName)
    {
        try
        {
            using (var fontCollection = new InstalledFontCollection())
            {
                foreach (var font in fontCollection.Families)
                {
                    if (font.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                    {
                        Log($"Font '{fontName}' is already installed. Installation cancelled.", LogFilePath);
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Log($"Failed to check if font is installed: {ex.Message}", LogFilePath);
            throw;
        }
    }

    private static void Log(string message, string logFilePath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? throw new InvalidOperationException("Log directory cannot be created."));
            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            StarZMessageBox.ShowDialog($"Failed to write to log file: {ex.Message}", "Error", false);
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool AddFontResource(string lpFileName);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
}
