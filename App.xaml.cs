using System.Windows;
using StarZLauncher.Utils;

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
            DiscordPresence.DiscordClient.Initialize();
            DiscordPresence.IdlePresence();
            hasRun = true;
        }
    }
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordPresence.StopPresence();
}
