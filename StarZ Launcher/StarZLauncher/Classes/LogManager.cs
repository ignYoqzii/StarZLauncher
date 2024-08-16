using StarZLauncher.Windows;
using System;
using System.IO;

public static class LogManager
{
    private static readonly string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Logs");

    static LogManager()
    {
        // Ensure log directory exists
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public static void Log(string message, string logFileName)
    {
        string logFilePath = Path.Combine(logDirectory, logFileName);

        try
        {
            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            // Handle logging errors gracefully
            StarZMessageBox.ShowDialog($"Failed to write to {logFilePath}: {ex.Message}", "Error", false);
        }
    }
}