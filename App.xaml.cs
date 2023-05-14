using System;
using System.IO;
using System.Net;
using System.Windows;
using StarZLauncher.Classes.Settings;
using StarZLauncher.Classes.Discord;

namespace StarZLauncher;

// class to create the launcher's folders in documents + check for launcher's version + call the DiscordRPC class for initializations based on the settings
public partial class App : Application
{
    bool HasRun = false;
    private const string? VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string starZLauncherPath = Path.Combine(documentsPath, "StarZ Launcher");
        string starZVersionsPath = Path.Combine(starZLauncherPath, "StarZ Versions");
        string starZScriptsPath = Path.Combine(starZLauncherPath, "StarZ Scripts");
        string dllsPath = Path.Combine(starZLauncherPath, "DLLs");
        string versionFilePath = Path.Combine(starZLauncherPath, "LauncherVersion.txt");
        string oldConfigFilePath = Path.Combine(starZLauncherPath, "Config.txt");

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


        if (!File.Exists(versionFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(versionFilePath));
            DownloadLatestVersion(versionFilePath); // Download the latest version from the URL and save it to the file
        }

        if (File.Exists(oldConfigFilePath))
        {
            File.Delete(oldConfigFilePath); // This deletes the old Config.txt from user's computer to replace it with the new config system (Settings.txt from ConfigTool)
        }

        if (!HasRun)
        {
            bool discordRPC = ConfigTool.GetDiscordRPC();
            if (discordRPC == true)
            {
                DiscordRichPresenceManager.DiscordClient.Initialize();
                DiscordRichPresenceManager.SetPresence();
                HasRun = true;
            }
            return;
        }
    }

    private static void DownloadLatestVersion(string filePath)
    {
        using WebClient client = new();
        string? latestVersion = client.DownloadString(VERSION_URL).Trim();
        File.WriteAllText(filePath, latestVersion); // Write the latest version to the file
    }
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordRichPresenceManager.TerminatePresence();
}
