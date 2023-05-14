using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using StarZLauncher.Classes;
using StarZLauncher.Classes.Game;
using StarZLauncher.Classes.Tabs;

namespace StarZLauncher.Windows;

public partial class MainWindow
{
    public static MainWindow? mainWindow;
    public static Label? DefaultDLLText;
    public static Label? CurrentMinecraftVersion;
    public static Label? CurrentLauncherVersion;
    public static StackPanel? FullVersionsListStackPanel;
    public static ListBox? DLLsListManager;
    public static Border? DragAndDropZone;
    private static readonly SettingsWindow SettingsWindow = new();

    public MainWindow()
    {
        InitializeComponent();
        FullVersionsListStackPanel = versionsStackPanel;
        DefaultDLLText = DefaultDLLLabel;
        CurrentMinecraftVersion = CurrentVersion;
        CurrentLauncherVersion = LabelVersion;
        DLLsListManager = DllList;
        DragAndDropZone = DragZone;
        SettingsWindow.Closing += OnClosing;
        Loader.Load();
    }

    public void MinimizeToTrayWindow()
    {
        Hide();
        SettingsWindow.Hide();
        Close();
    }

    public void MinimizeWindow()
    {
        WindowState = WindowState.Minimized;
        SettingsWindow.Hide();
    }

    // Animation on program's launch

    private bool isFirstTimeOpened = true;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (isFirstTimeOpened)
        {
            DoubleAnimation animation = new(0, 1, new Duration(TimeSpan.FromSeconds(1)))
            {
                EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.OpacityProperty, animation);

            // Wait for 3 seconds before setting the visibility of the grid to Hidden
            await Task.Delay(2000);

            DoubleAnimation opacityAnimation = new()
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
            };
            OpeningAnim.BeginAnimation(Image.OpacityProperty, opacityAnimation);

            await Task.Delay(500);
            // Handle the Completed event to hide the grid when the opacity animation is finished
            OpeningAnim.Visibility = Visibility.Collapsed;

            isFirstTimeOpened = false;
        }
    }

    // show the settings window on click of the settings icon
    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        {
            // Set the WindowStartupLocation property of the new window to Manual
            SettingsWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // Set the Top and Left properties of the new window to the same values as the current window
            SettingsWindow.Top = this.Top;
            SettingsWindow.Left = this.Left;
        }

        // Show the settings window
        SettingsWindow.Show();
    }

    // close the program
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    // minimize the program
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    // move the window on screen
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;

    /// <summary>



    // MainMenuTab section code



    /// </summary>

    //This is the event for the little eye icon to hide or show the news
    private void TogglePanels_Click(object sender, RoutedEventArgs e)
    {
        if (PanelA.Visibility == Visibility.Visible)
        {
            PanelA.Visibility = Visibility.Collapsed;
        }
        else if (PanelA.Visibility == Visibility.Collapsed)
        {
            PanelA.Visibility = Visibility.Visible;
        }
    }

    private void ShowFullVersionsList_Click(object sender, RoutedEventArgs e) 
    {
        MinecraftVersionsListManager.LoadVersionsManager();
        MainMenu.Visibility = Visibility.Collapsed;
        FullVersionsList.Visibility = Visibility.Visible;
    }

    private void GoBackFullVersionList_MouseLeftButtonDown(object sender,  RoutedEventArgs e) 
    {
        FullVersionsList.Visibility = Visibility.Collapsed;
        MainMenu.Visibility = Visibility.Visible;
    }

    //Only run Minecraft without DLLs or with the default dll
    public void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        Launch.LaunchGameOnLeftClick();
    }


    //Run Minecraft with a DLL selected from the file explorer window
    public void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        Launch.LaunchGameOnRightClick();
    }

    //To download the appxs
    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        MinecraftVersionsListManager.DownloadCustomVersions(sender);
    }

    //To install the zips after download
    private void InstallButton_Click(object sender, EventArgs e)
    {
        MinecraftVersionsListManager.InstallMinecraftPackage();
    }

    /// <summary>



    // Mods Manager section code



    /// </summary>

    private void GetScriptsButton_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://github.com/OnixClient-Scripts/OnixClient_Scripts");

    //Open the resourcepacks folder
    private void TexturePackButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        string minecraftFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs";

        if (Directory.Exists(minecraftFolderPath))
        {
            Process.Start(minecraftFolderPath);
        }
        else
        {
            MessageBox.Show("Error while opening a folder ; is Minecraft installed?");
        }
    }

    // persona skin pack installer if StarZ X Minecraft is installed
    private void Persona_Click(object sender, RoutedEventArgs e)
    {
        ToolsManager.CosmeticsSkinPackApply();
    }

    // shader materials.bin installer if StarZ X Minecraft is installed
    private void ShaderInstall_Click(object sender, RoutedEventArgs e)
    {
        ToolsManager.ShaderApply();
    }

    // remove any installed shader
    private void ShaderRemove_Click(object sender, RoutedEventArgs e)
    {
        ToolsManager.ShaderRemove();
    }

    /// <summary>



    // DLLs Clients section code



    /// </summary>

    // To edit the name of a dll
    private void Edit_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        DLLsManager.Edit();
    }

    // set the selected dll as default
    private void SetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        DLLsManager.SetDefaultDLL();
    }

    // remove the default dll
    private void ResetSetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        DLLsManager.Reset();
    }

    // delete the selected dll
    private void Delete_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        DLLsManager.Delete();
    }

    private void AddDllsButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        DLLsManager.Add();
    }

    /// <summary>



    // About section code



    /// </summary>

    private void LauncherFolder_Click(object sender, RoutedEventArgs e)
    {
        string LauncherFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher";

        if (Directory.Exists(LauncherFolderPath))
        {
            Process.Start(LauncherFolderPath);
        }
        else
        {
            MessageBox.Show("Error while opening a folder ; restart the application. If the issue persists, ask for #support on our Discord server.");
        }
    }

    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/ScR9MGbRSY");

    private void YouTubeIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.youtube.com/channel/UCbN3FxySrPSeUMVe5ISraWw");

    private void GithubIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/ignYoqzii/StarZLauncher");

    private void TwitchIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("https://www.twitch.tv/zl1me");
}