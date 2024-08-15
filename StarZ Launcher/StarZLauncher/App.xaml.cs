using StarZLauncher.Classes;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace StarZLauncher;

public partial class App : Application
{
    bool HasRun = false;
    private const string? VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        bool debug = ConfigManager.GetOfflineMode();
        if (debug == false)
        {
            Task.Run(() => FontInstaller.FontInstallation());
        }
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string starZLauncherPath = Path.Combine(documentsPath, "StarZ Launcher");
        string VersionsPath = Path.Combine(starZLauncherPath, "Versions");
        string profilesPath = Path.Combine(starZLauncherPath, "Profiles");
        string musicsPath = Path.Combine(starZLauncherPath, "Musics");
        string dllsPath = Path.Combine(starZLauncherPath, "DLLs");
        string logsPath = Path.Combine(starZLauncherPath, "Logs");
        string versionFilePath = Path.Combine(starZLauncherPath, "LauncherVersion.txt");
        string oldConfigFilePath = Path.Combine(starZLauncherPath, "Config.txt");

        if (!Directory.Exists(starZLauncherPath))
        {
            Directory.CreateDirectory(starZLauncherPath);
        }

        if (!Directory.Exists(VersionsPath))
        {
            Directory.CreateDirectory(VersionsPath);
        }

        if (!Directory.Exists(dllsPath))
        {
            Directory.CreateDirectory(dllsPath);
        }

        if (!Directory.Exists(profilesPath))
        {
            Directory.CreateDirectory(profilesPath);
        }

        if (!Directory.Exists(musicsPath))
        {
            Directory.CreateDirectory(musicsPath);
        }

        if (!Directory.Exists(logsPath))
        {
            Directory.CreateDirectory(logsPath);
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
            bool discordRPC = ConfigManager.GetDiscordRPC();
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

