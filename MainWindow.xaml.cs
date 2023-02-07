using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using StarZLauncher.Utils;
using Microsoft.Win32;
using System.Windows.Media.Animation;

namespace StarZLauncher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static Process? Minecraft;
    private static readonly NewsWindow NewsWindow = new();
    private static readonly CreditWindow CreditWindow = new();
    private static readonly SettingsWindow SettingsWindow = new();
    public static bool IsMinecraftRunning;

    public MainWindow()
    {
        InitializeComponent();
        DiscordPresence.DiscordClient.Initialize();
        DiscordPresence.IdlePresence();
        NewsWindow.Closing += OnClosing;
        CreditWindow.Closing += OnClosing;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private async void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        SetStatusLabel.Pending("Opening DLL...");

        OpenFileDialog openFileDialog = new()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true)
        {
            SetStatusLabel.Default();
            return;
        }

        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }

        await Injector.WaitForModules();
        Injector.Inject(openFileDialog.FileName);
        IsMinecraftRunning = true;

        Minecraft.EnableRaisingEvents = true;
        Minecraft.Exited += IfMinecraftExited;
    }
    private async void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        SetStatusLabel.Pending("Opening DLL...");
        
        OpenFileDialog openFileDialog = new()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true)
        {
            SetStatusLabel.Default();
            return;
        }
        
        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }

        await Injector.WaitForModules();
        Injector.Inject(openFileDialog.FileName);
        IsMinecraftRunning = true;

        Minecraft.EnableRaisingEvents = true;
        Minecraft.Exited += IfMinecraftExited;
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        DiscordPresence.DiscordClient.UpdateState("Idling in the launcher");
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
        IsMinecraftRunning = false;
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        NewsWindow.Show();
        DiscordPresence.DiscordClient.UpdateState("Reading the launcher's news");
    }

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreditWindow.Show();
        DiscordPresence.DiscordClient.UpdateState("Reading the launcher's credits");
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        SettingsWindow.Show();
        DiscordPresence.DiscordClient.UpdateState("In the launcher's settings");
    }

    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/Discord.txt?token=GHSAT0AAAAAAB6MIQ3XTE73B2RMYM6V2JRGY7BTRLQ");

    private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

    private void OnixIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/onixclient");

    private void LatiteIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/latite");

    private void LuconiaIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/luconia");

    private void DownloadIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");

    private void MC19Icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://feedback.minecraft.net/hc/en-us/articles/11394437843341-Minecraft-1-19-51-Bedrock-");

    private void MC18Icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://feedback.minecraft.net/hc/en-us/sections/360001185332-Beta-and-Preview-Information-and-Changelogs");

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DoubleAnimation animation = new DoubleAnimation();
        animation.From = 0;
        animation.To = 1;
        animation.Duration = new Duration(TimeSpan.FromSeconds(2));
        this.BeginAnimation(Window.OpacityProperty, animation);
    }

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}