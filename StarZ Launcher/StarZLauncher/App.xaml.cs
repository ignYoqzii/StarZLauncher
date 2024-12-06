using StarZLauncher.Classes;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace StarZLauncher;

public partial class App : Application
{
    private bool HasRun = false;
    private const string VERSION_URL = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";
    private static readonly string logFileName = $"AppStartup.txt";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            bool debug = ConfigManager.GetDebugFontInstaller();
            if (!debug)
            {
                Task.Run(() => FontInstaller.FontInstallation());
            }
            else
            {
                LogManager.Log("Font installation is disabled. To re-enable it, change 'DebugFontInstaller' value in Settings.txt to 'False'.", "FontInstaller.txt");
            }

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string starZLauncherPath = Path.Combine(documentsPath, "StarZ Launcher");
            string VersionsPath = Path.Combine(starZLauncherPath, "Versions");
            string profilesPath = Path.Combine(starZLauncherPath, "Profiles");
            string musicsPath = Path.Combine(starZLauncherPath, "Musics");
            string dllsPath = Path.Combine(starZLauncherPath, "Mods");
            string logsPath = Path.Combine(starZLauncherPath, "Logs");
            string versionFilePath = Path.Combine(starZLauncherPath, "LauncherVersion.txt");
            string oldConfigFilePath = Path.Combine(starZLauncherPath, "Config.txt");

            EnsureDirectoryExists(starZLauncherPath);
            EnsureDirectoryExists(VersionsPath);
            EnsureDirectoryExists(dllsPath);
            EnsureDirectoryExists(profilesPath);
            EnsureDirectoryExists(musicsPath);
            EnsureDirectoryExists(logsPath);

            if (!File.Exists(versionFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(versionFilePath));
                DownloadLatestVersion(versionFilePath);
            }

            if (File.Exists(oldConfigFilePath))
            {
                File.Delete(oldConfigFilePath);
            }

            if (!HasRun)
            {
                bool discordRPC = ConfigManager.GetDiscordRPC();
                bool offlinemode = ConfigManager.GetOfflineMode();
                if (discordRPC && !offlinemode)
                {
                    DiscordRichPresenceManager.DiscordClient.Initialize();
                    DiscordRichPresenceManager.SetPresence();
                    HasRun = true;
                    LogManager.Log("Initialized Discord Rich Presence.", logFileName);
                }
                else
                {
                    LogManager.Log("Could not initialize Discord Rich Presence. This is either because Discord RPC was disabled or Offline Mode is enabled.", logFileName);
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log($"Error during application startup: {ex.Message}", logFileName);
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            LogManager.Log($"Created directory: {path}", logFileName);
        }
    }

    private static void DownloadLatestVersion(string filePath)
    {
        try
        {
            using WebClient client = new();
            string latestVersion = client.DownloadString(VERSION_URL).Trim();
            File.WriteAllText(filePath, latestVersion);
            LogManager.Log($"Downloaded and updated version file: {filePath}", logFileName);
        }
        catch (Exception ex)
        {
            LogManager.Log($"Error downloading latest version: {ex.Message}", logFileName);
        }
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
        try
        {
            DiscordRichPresenceManager.TerminatePresence();
            LogManager.Log("Application exited successfully.", logFileName);
        }
        catch (Exception ex)
        {
            LogManager.Log($"Error during application exit: {ex.Message}", logFileName);
        }
    }
}
