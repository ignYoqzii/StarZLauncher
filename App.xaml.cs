using System.Windows;
using StarZLauncher.Utils;

namespace StarZLauncher;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordPresence.StopPresence();
}