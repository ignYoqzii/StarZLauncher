using System;
using System.IO;
using System.Net;
using System.Windows;
using StarZLauncher.Classes;

namespace StarZLauncher;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static bool hasRun = false;
    private const string? VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!hasRun)
        {
            DiscordRichPresenceManager.DiscordClient.Initialize();
            DiscordRichPresenceManager.IdlePresence();
            hasRun = true;
        }

        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string starZLauncherPath = Path.Combine(documentsPath, "StarZ Launcher");
        string starZVersionsPath = Path.Combine(starZLauncherPath, "StarZ Versions");
        string starZScriptsPath = Path.Combine(starZLauncherPath, "StarZ Scripts");
        string dllsPath = Path.Combine(starZLauncherPath, "DLLs");
        string configPath = Path.Combine(starZLauncherPath, "Config.txt");
        string versionFilePath = Path.Combine(starZLauncherPath, "LauncherVersion.txt");

        if (!Directory.Exists(starZLauncherPath))
        {
            Directory.CreateDirectory(starZLauncherPath);
        }

        if (!Directory.Exists(starZVersionsPath))
        {
            Directory.CreateDirectory(starZVersionsPath);
        }

        if (!Directory.Exists(starZScriptsPath))
        {
            Directory.CreateDirectory(starZScriptsPath);
        }

        if (!Directory.Exists(dllsPath))
        {
            Directory.CreateDirectory(dllsPath);
        }

        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, "DefaultDLL: None");
        }

        if (!File.Exists(versionFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(versionFilePath));
            DownloadLatestVersion(versionFilePath); // Download the latest version from the URL and save it to the file
        }
    }

    private static void DownloadLatestVersion(string filePath)
    {
        using WebClient client = new();
        string? latestVersion = client.DownloadString(VERSION_URL).Trim();
        File.WriteAllText(filePath, latestVersion); // Write the latest version to the file
    }
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordRichPresenceManager.StopPresence();
}
