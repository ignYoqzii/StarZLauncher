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
    private static readonly CreditWindow CreditWindow = new();
    private static readonly SettingsWindow SettingsWindow = new();
    public static bool IsMinecraftRunning;

    public MainWindow()
    {
        InitializeComponent();
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

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
        CreditWindow.Show();
        if (MainWindow.IsMinecraftRunning)
            DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
        else DiscordPresence.DiscordClient.UpdateState("Reading the launcher's credits");
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
        SettingsWindow.Show();
        if (MainWindow.IsMinecraftRunning)
            DiscordPresence.DiscordClient.UpdateState($"Playing Minecraft 1.19.51");
        else DiscordPresence.DiscordClient.UpdateState("In the launcher's settings");
    }

    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/Discord.txt?token=GHSAT0AAAAAAB6MIQ3XTE73B2RMYM6V2JRGY7BTRLQ");

    private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

    private void OnixIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/onixclient");

    private void LatiteIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/latite");

    private void LuconiaIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/luconia");

    private void DownloadIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");

    private void MC19Icon_MouseLeftButtonDown(object sender, RoutedEventArgs e) => Process.Start("https://feedback.minecraft.net/hc/en-us/sections/360001186971-Release-Changelogs");

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DoubleAnimation animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(2)));
        animation.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
        this.BeginAnimation(Window.OpacityProperty, animation);
    }

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}