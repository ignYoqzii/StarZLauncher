using System;
using System.IO;
using System.Windows;
using StarZLauncher.Classes;

namespace StarZLauncher;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static bool hasRun = false;

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
    }
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordRichPresenceManager.StopPresence();
}
