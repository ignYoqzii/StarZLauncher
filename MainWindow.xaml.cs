using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using StarZLauncher.Classes;

namespace StarZLauncher;

public partial class MainWindow
{
    public static Process? Minecraft;
    private bool isRunning = false;
    public static readonly versionInfo versionInfo = new();
    readonly string? versionNumber = versionInfo.VersionNumber;
    readonly string? launcherVersion = versionInfo.LauncherVersion;
    private static readonly SettingsWindow SettingsWindow = new();
    public static bool IsMinecraftRunning;
    private readonly string DllsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "DLLs");
    private readonly string starzScriptsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher\StarZ Scripts\";
    private readonly string resourcePacksFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs\";
    private string? defaultDll;
    public static string? DllNameLaunchOnLeftClick { get; private set; }
    public static string? DllNameLaunchOnRightClick { get; private set; }
    private const string LatestVersionUrl = "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/latest_version.txt";
    private const string DownloadBaseUrl = "https://github.com/Imrglop/Latite-Releases/releases/download/{0}/Latite.{1}.dll";
    private const string DLL_FOLDER = @"StarZ Launcher\DLLs";
    private readonly ObservableCollection<string> _dlls = new();

    public MainWindow()
    {
        InitializeComponent();
        SetBackgroundImage();
        SettingsWindow.Closing += OnClosing;
        InitializeDragDrop();
        LoadDlls();
        DllList.ItemsSource = _dlls;
        LoadCurrentVersion();
        LoadDefaultDLL();
        // Call CheckForUpdates to check for updates on startup
        versionInfo.CheckForUpdates();
    }

    private void LoadDefaultDLL()
    {
        defaultDll = ConfigTool.GetDefaultDLL();
        DefaultDLLLabel.Content = $"Default DLL: {defaultDll}";
    }
    private void LoadCurrentVersion()
    {
        if (versionInfo.filePath != null)
        {
            CurrentVersion.Content = $"Current Version: {versionNumber}";
            LabelVersion.Content = $"{launcherVersion}";
        }
        else
        {
            CurrentVersion.Content = "Current Version: Error";
            LabelVersion.Content = "Error";
        }
    }

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

    //Needs some work
    private void TogglePanels_Click(object sender, RoutedEventArgs e)
    {
        if (PanelB.Visibility == Visibility.Visible)
        {
            PanelA.Visibility = Visibility.Visible;
            PanelB.Visibility = Visibility.Collapsed;
        }
        else if (PanelA.Visibility == Visibility.Visible && PanelB.Visibility == Visibility.Collapsed)
        {
            PanelA.Visibility = Visibility.Collapsed;
            PanelB.Visibility = Visibility.Collapsed;
        }
        else
        {
            PanelA.Visibility = Visibility.Visible;
            PanelB.Visibility = Visibility.Collapsed;
        }
    }

    private void LoadDlls()
    {
        string dllFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            DLL_FOLDER);

        if (Directory.Exists(dllFolderPath))
        {
            string[] dllFiles = Directory.GetFiles(dllFolderPath, "*.dll");
            foreach (string dllFile in dllFiles)
            {
                _dlls.Add(Path.GetFileName(dllFile));
            }
        }
    }

    private void Edit_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        string selectedDll = (string)DllList.SelectedItem ?? throw new ArgumentNullException(nameof(DllList.SelectedItem));
        string currentName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER, selectedDll);

        RenameWindow renameWindow = new(selectedDll);
        _ = renameWindow.ShowDialog();
        string newName = renameWindow.NewName ?? throw new ArgumentNullException(nameof(renameWindow.NewName));

        string newFullName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DLL_FOLDER, newName);

        File.Move(currentName, newFullName);

        int selectedIndex = DllList.SelectedIndex;
        _dlls[selectedIndex] = newName;

        if (selectedDll == defaultDll)
        {
            ConfigTool.SetDefaultDLL(newName);
            LoadDefaultDLL();
        }
    }


    private void SetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        string selectedItem = (string)DllList.SelectedItem;
        if (selectedItem == null) return;

        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Config.txt");
        if (!File.Exists(configPath)) return;

        var result = MessageBox.Show($"Are you sure you want to set the default DLL to {selectedItem}? This will automatically inject your DLL when launching.", "Confirmation", MessageBoxButton.OKCancel);
        if (result != MessageBoxResult.OK) return;

        ConfigTool.SetDefaultDLL(selectedItem);
        LoadDefaultDLL();
    }

    private void ResetSetDefaultDLLButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Config.txt");
        if (!File.Exists(configPath)) return;

        var result = MessageBox.Show($"Are you sure you want to reset the default DLL to None? Doing so will remove the auto-injection on launch.", "Confirmation", MessageBoxButton.OKCancel);
        if (result != MessageBoxResult.OK) return;

        ConfigTool.SetDefaultDLL("None");
        LoadDefaultDLL();
    }

    private void Delete_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        string selectedDll = (string)DllList.SelectedItem;
        if (selectedDll != null)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this DLL?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    DLL_FOLDER,
                    selectedDll);
                File.Delete(filePath);

                _dlls.Remove(selectedDll);
            }
            if (selectedDll == defaultDll)
            {
                ConfigTool.SetDefaultDLL("None");
                LoadDefaultDLL();
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
    //Play Section (Main Menu)
    //Only run Minecraft without DLLs
    public async void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        if (Process.GetProcessesByName("Minecraft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        Minecraft = await Task.Run(() =>
        {
            while (Process.GetProcessesByName("Minecraft.Windows").Length == 0) { }
            return Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();
        });
        if (Minecraft == null) return;

        try
        {
            LoadDefaultDLL();
            if (defaultDll == "None")
            {
                // Launch the game without injecting
                IsMinecraftRunning = true;
                Minecraft.EnableRaisingEvents = true;
                Minecraft.Exited += IfMinecraftExited;
            }
            else if (!string.IsNullOrEmpty(defaultDll))
            {
                // Check if the DLL file exists in the specified location
                string dllFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "DLLs", defaultDll);
                if (File.Exists(dllFilePath))
                {
                    try
                    {
                        // Inject the specified DLL file
                        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(dllFilePath);
                        DllNameLaunchOnLeftClick = filenameWithoutExtension;
                        bool DiscordRPCisEnabled = ConfigTool.GetDiscordRPC();
                        bool DiscordShowGameVersionisEnabled = ConfigTool.GetDiscordRPCShowGameVersion();
                        bool DiscordShowDLLNameisEnabled = ConfigTool.GetDiscordRPCShowDLLName();
                        if (DiscordRPCisEnabled == true)
                        {
                            if (DiscordShowGameVersionisEnabled == true)
                            {
                                DiscordRichPresenceManager.DiscordClient.UpdateDetails($"Playing Minecraft {versionNumber}");
                            }
                            else if (DiscordShowGameVersionisEnabled == false)
                            {
                                DiscordRichPresenceManager.DiscordClient.UpdateDetails("Playing Minecraft");
                            }
                            if (DiscordShowDLLNameisEnabled == true)
                            {
                                DiscordRichPresenceManager.DiscordClient.UpdateState($"With {DllNameLaunchOnLeftClick}");
                            }
                            else if (DiscordShowDLLNameisEnabled == false)
                            {
                                DiscordRichPresenceManager.DiscordClient.UpdateState("");
                            }
                        }

                        string LaunchOptionisSelected = ConfigTool.GetLaunchOption();
                        if (LaunchOptionisSelected == "MinimizeToTray")
                        {
                            TrayManager.MinimizeToTray();
                            this.Hide();
                            SettingsWindow.Hide();
                            this.Close();
                        }
                        else if (LaunchOptionisSelected == "Minimize")
                        {
                            WindowState = WindowState.Minimized;
                            SettingsWindow.Hide();
                        }
                        await Injector.WaitForModules();
                        Injector.Inject(dllFilePath);
                        IsMinecraftRunning = true;

                        Minecraft.EnableRaisingEvents = true;
                        Minecraft.Exited += IfMinecraftExited;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to inject DLL file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"The specified DLL file '{defaultDll}' does not exist in the 'StarZ Launcher/DLLs' folder. Make sure your DLL is located in that folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The 'Config.txt' file does not specify a default DLL file or the 'DefaultDLL' line is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to read the 'Config.txt' file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    //Run Minecraft with a DLL
    public async void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true,
            InitialDirectory = DllsFolderPath
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        if (Process.GetProcessesByName("Minecraft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        Minecraft = await Task.Run(() =>
        {
            while (Process.GetProcessesByName("Minecraft.Windows").Length == 0) { }
            return Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();
        });
        if (Minecraft == null) return;

        try
        {
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            DllNameLaunchOnRightClick = filenameWithoutExtension;
            bool DiscordRPCisEnabled = ConfigTool.GetDiscordRPC();
            bool DiscordShowGameVersionisEnabled = ConfigTool.GetDiscordRPCShowGameVersion();
            bool DiscordShowDLLNameisEnabled = ConfigTool.GetDiscordRPCShowDLLName();
            if (DiscordRPCisEnabled == true)
            {
                if (DiscordShowGameVersionisEnabled == true)
                {
                    DiscordRichPresenceManager.DiscordClient.UpdateDetails($"Playing Minecraft {versionNumber}");
                }
                else if (DiscordShowGameVersionisEnabled == false)
                {
                    DiscordRichPresenceManager.DiscordClient.UpdateDetails("Playing Minecraft");
                }
                if (DiscordShowDLLNameisEnabled == true)
                {
                    DiscordRichPresenceManager.DiscordClient.UpdateState($"With {DllNameLaunchOnRightClick}");
                }
                else if (DiscordShowDLLNameisEnabled == false)
                {
                    DiscordRichPresenceManager.DiscordClient.UpdateState("");
                }
            }

            string LaunchOptionisSelected = ConfigTool.GetLaunchOption();
            if (LaunchOptionisSelected == "MinimizeToTray")
            {
                TrayManager.MinimizeToTray();
                this.Hide();
                SettingsWindow.Hide();
                this.Close();
            }
            else if (LaunchOptionisSelected == "Minimize")
            {
                WindowState = WindowState.Minimized;
                SettingsWindow.Hide();
            }
            await Injector.WaitForModules();
            Injector.Inject(openFileDialog.FileName);
            IsMinecraftRunning = true;

            Minecraft.EnableRaisingEvents = true;
            Minecraft.Exited += IfMinecraftExited;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to inject DLL file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void IfMinecraftExited(object sender, EventArgs e)
    {
        bool DiscordRPCisEnabled = ConfigTool.GetDiscordRPC();
        if (DiscordRPCisEnabled == true)
        {
            DiscordRichPresenceManager.IdlePresence();
        }
        IsMinecraftRunning = false;
    }

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

    public void SetBackgroundImage()
    {
        // Read file contents (image file name)
        string fileName = ConfigTool.GetLauncherBackground();

        // Create file path for image file
        string imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Images", fileName);

        // Check if image file exists
        if (File.Exists(imagePath))
        {
            // Load image into image control
            BitmapImage image = new(new Uri(imagePath));
            BackgroundImage.Source = image;
        }
    }

    private readonly Dictionary<Button, string> downloads = new();
    private bool isDownloading = false;

    //To download the appxs
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        var downloadButton = (Button)sender;
        var version = (string)downloadButton.Tag;
        var fileUrl = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/versionsselectorv2/StarZXMinecraft-{version}.zip";
        var fileName = $"StarZXMinecraft-{version}.zip";
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions");

        if (isDownloading || downloads.ContainsValue(fileUrl))
        {
            return; // Do nothing if a download is already in progress or the file is already being downloaded
        }

        try
        {
            isDownloading = true;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            // Check for internet connectivity
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("No internet!");
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Delete the file if it already exists
            }

            using (var webClient = new WebClient())
            {
                var downloadProgressWindow = new DownloadProgressWindow
                {
                    Owner = this // Set the owner of the progress window to be the main window
                };
                downloadProgressWindow.Show();

                webClient.DownloadProgressChanged += (s, e) => downloadProgressWindow.progressBar.Value = e.ProgressPercentage;

                var downloadTask = webClient.DownloadFileTaskAsync(new Uri(fileUrl), filePath);

                downloadProgressWindow.CancelButton.Click += (s, e) =>
                {
                    webClient.CancelAsync(); // Cancel the download if the cancel button is clicked
                    downloadTask.ContinueWith(t => File.Delete(filePath)); // Delete the file if the download is canceled
                    downloadProgressWindow.Close();
                };

                await downloadTask;

                downloadProgressWindow.Close();
            }

            downloads.Add(downloadButton, fileUrl);

            WebClient_DownloadFileCompleted(downloadButton);
        }
        catch (Exception)
        {
            MessageBox.Show($"Canceled!");
        }
        finally
        {
            isDownloading = false;
        }
    }

    private void WebClient_DownloadFileCompleted(Button downloadButton)
    {
        downloads.Remove(downloadButton);

        if (downloads.Count == 0)
        {
            // Show a message box to inform the user that the download has finished
            MessageBox.Show("Download completed!");
        }
    }

    //To install the appxs
    private void InstallButton_Click(object sender, EventArgs e)
    {
        if (isRunning) // check if installation is already in progress or not
        {
            return;
        }

        // show warning message box to the user
        MessageBoxResult result = MessageBox.Show("Warning: This will uninstall your current Microsoft Store Minecraft version and install a modified version of it. If Minecraft is installed somewhere else on your computer, uninstall it to avoid problems. Backup your files if needed! Also, make sure PowerShell is installed on your computer! Are you sure you want to proceed?", "StarZ X Minecraft", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel)
        {
            return; // user clicked cancel, so do nothing
        }

        isRunning = true; // set installation in progress flag

        // show file dialog to the user to select the zip file
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Zip Files|*.zip",
            InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "StarZ Versions")
        };
        bool? _ = openFileDialog.ShowDialog();

        if (!_ == true)
        {
            isRunning = false; // reset installation in progress flag
            return;
        }

        SetStatusLabelText("Installing... Please wait...", Colors.Yellow);
        // unregister Minecraft from Microsoft Store
        string unregistercommand = $"Get-AppxPackage *minecraftUWP* | Remove-AppxPackage";
        ProcessStartInfo psiunregister = new("powershell.exe", unregistercommand)
        {
            UseShellExecute = true,
            Verb = "runas" // Run PowerShell as administrator
        };
        Process.Start(psiunregister).WaitForExit();

        string selectedZipFile = openFileDialog.FileName;
        string extractedFolderPath = Path.Combine(Path.GetDirectoryName(selectedZipFile), "StarZ X Minecraft");

        // delete the existing StarZ X Minecraft folder if it exists (so it uninstall any previous StarZ X Minecraft version)
        if (Directory.Exists(extractedFolderPath))
        {
            Directory.Delete(extractedFolderPath, true);
        }

        // create a new StarZ X Minecraft folder
        Directory.CreateDirectory(extractedFolderPath);

        // move the selected zip file to the StarZ X Minecraft folder
        string newZipFileName = Path.Combine(extractedFolderPath, "StarZXMinecraft.zip");
        File.Move(selectedZipFile, newZipFileName);

        // extract the zip file
        ZipFile.ExtractToDirectory(newZipFileName, extractedFolderPath);

        // register the app using Powershell command
        string manifestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ` Launcher", "StarZ` Versions", "StarZ` X` Minecraft", "AppxManifest.xml");
        string registercommand = $"Add-AppxPackage -Path {manifestPath} -Register";
        SetStatusLabelText("Registering...", Colors.Green);

        ProcessStartInfo psiregister = new("powershell.exe", registercommand)
        {
            UseShellExecute = true,
            Verb = "runas" // Run PowerShell as administrator
        };
        Process.Start(psiregister).WaitForExit();

        // reset the label text and color
        SetStatusLabelText("", Colors.AliceBlue);

        // show success message box to the user
        MessageBox.Show("StarZ X Minecraft has been installed successfully! It is recommended to launch the game without the launcher first to avoid any bugs...", "StarZ X Minecraft", MessageBoxButton.OK, MessageBoxImage.Information);

        // delete the initial renamed zip file if it still exists
        if (File.Exists(newZipFileName))
        {
            File.Delete(newZipFileName);
        }

        isRunning = false; // reset installation in progress flag
    }

    private void SetStatusLabelText(string text, Color color)
    {
        Dispatcher.Invoke(() =>
        {
            statusLabel.Content = text;
            statusLabel.Foreground = new SolidColorBrush(color);
        });
    }

    //Scripts / Mods Section
    private void GetScriptsButton_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://github.com/bernarddesfosse/OnixClient_Scripts");

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
    private void InitializeDragDrop()
    {
        DragZone.AllowDrop = true;
        DragZone.DragEnter += DragZone_DragEnter;
        DragZone.DragLeave += DragZone_DragLeave;
        DragZone.Drop += DragZone_Drop;
    }

    private void DragZone_DragEnter(object sender, DragEventArgs e)
    {
        if (IsLuaFile(e) || IsMcpackOrZipFile(e))
        {
            DragZone.BorderThickness = new Thickness(2);
            DragZone.BorderBrush = Brushes.White;
        }
    }

    private void DragZone_DragLeave(object sender, DragEventArgs e)
    {
        DragZone.BorderThickness = new Thickness(0.5);
        DragZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
    }

    private void DragZone_Drop(object sender, DragEventArgs e)
    {
        if (IsLuaFile(e))
        {
            MoveLuaFile(e);
        }
        else if (IsMcpackOrZipFile(e))
        {
            ExtractMcpackOrZipFile(e);
        }

        DragZone.BorderThickness = new Thickness(0.5);
        DragZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
    }

    private bool IsLuaFile(DragEventArgs e)
    {
        return e.Data.GetDataPresent(DataFormats.FileDrop) &&
               IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".lua");
    }

    private bool IsMcpackOrZipFile(DragEventArgs e)
    {
        return e.Data.GetDataPresent(DataFormats.FileDrop) &&
               (IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".mcpack") ||
                IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".zip"));
    }

    private bool IsFileOfType(string filePath, string extension)
    {
        return Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase);
    }

    private void MoveLuaFile(DragEventArgs e)
    {
        string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
        if (!Directory.Exists(starzScriptsFolder))
        {
            Directory.CreateDirectory(starzScriptsFolder);
        }
        File.Move(filePath, starzScriptsFolder + Path.GetFileName(filePath));
    }

    private void ExtractMcpackOrZipFile(DragEventArgs e)
    {
        string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
        string extractPath = resourcePacksFolder + Path.GetFileNameWithoutExtension(filePath);

        try
        {
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            if (IsMcpackFile(filePath))
            {
                ZipFile.ExtractToDirectory(filePath, extractPath);
            }
            else if (IsZipFile(filePath))
            {
                ZipFile.ExtractToDirectory(filePath, extractPath);
            }

            // Delete the file after extraction is complete
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error extracting file: " + ex.Message);
        }
    }

    private bool IsMcpackFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        return extension.Equals(".mcpack", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsZipFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
    }

    private void Persona_Click(object sender, RoutedEventArgs e)
    {
        string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "skin_packs");

        if (!Directory.Exists(destinationPath))
        {
            MessageBox.Show("StarZ X Minecraft is not installed!");
            return;
        }

        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select persona folder"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string sourcePath = dialog.SelectedPath;
            string sourceFolderName = new DirectoryInfo(sourcePath).Name;
            string destinationFolder = Path.Combine(destinationPath, sourceFolderName);

            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
            }

            Directory.Move(sourcePath, destinationFolder);
            MessageBox.Show("Custom Persona installed successfully!");
        }
    }

    private void ShaderInstall_Click(object sender, RoutedEventArgs e)
    {
        string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "renderer", "materials");
        string backupPath = Path.Combine(destinationPath, "Backup");

        // Check if destination directory exists
        if (!Directory.Exists(destinationPath))
        {
            MessageBox.Show("StarZ X Minecraft is not installed!");
            return;
        }

        // Create backup directory if it doesn't exist
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }

        if (Directory.GetFiles(backupPath).Length > 0)
        {
            MessageBox.Show("Shaders are already installed!");
            return;
        }

        // Move 4 files to backup directory
        string[] filesToMove = new string[]
        {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
        };
        foreach (string file in filesToMove)
        {
            string sourceFilePath = Path.Combine(destinationPath, file);
            string destinationFilePath = Path.Combine(backupPath, file);
            if (File.Exists(sourceFilePath))
            {
                File.Move(sourceFilePath, destinationFilePath);
            }
        }

        // Download 4 files
        string[] filesToDownload = new string[]
        {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
        };
        foreach (string file in filesToDownload)
        {
            string url = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/shadersinstaller/{file}";
            string destinationFilePath = Path.Combine(destinationPath, file);
            using WebClient client = new();
            client.DownloadFile(url, destinationFilePath);
        }

        MessageBox.Show("Shaders installed successfully!");
    }

    private void ShaderRemove_Click(object sender, RoutedEventArgs e)
    {
        string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "renderer", "materials");
        string backupPath = Path.Combine(destinationPath, "Backup");

        // Check if StarZ X Minecraft is installed
        if (!Directory.Exists(destinationPath))
        {
            MessageBox.Show("StarZ X Minecraft is not installed!");
            return;
        }

        // Check if Backup folder exists
        if (!Directory.Exists(backupPath))
        {
            MessageBox.Show("No shaders previously installed.");
            return;
        }

        // Delete 4 files from destination folder
        string[] filesToDelete = new string[]
        {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
        };
        foreach (string file in filesToDelete)
        {
            string filePath = Path.Combine(destinationPath, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        // Move 4 files from backup folder to destination folder
        string[] filesToMove = new string[]
        {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
        };
        foreach (string file in filesToMove)
        {
            string sourceFilePath = Path.Combine(backupPath, file);
            string destinationFilePath = Path.Combine(destinationPath, file);
            if (File.Exists(sourceFilePath))
            {
                File.Move(sourceFilePath, destinationFilePath);
            }
        }

        MessageBox.Show("Shaders removed successfully!");
    }

    //DLLs Section
    // Download Latite's DLLs and launch Minecraft if checkbox is checked
    private void DownloadButtonLatite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        string minecraftVersion = button.Tag.ToString();
        string? latestVersion = GetLatestVersion();
        if (latestVersion == null)
        {
            MessageBox.Show("Failed to retrieve latest version information.");
            return;
        }

        string downloadUrl = string.Format(DownloadBaseUrl, latestVersion, minecraftVersion);

        using WebClient webClient = new();
        try
        {
            byte[] data = webClient.DownloadData(downloadUrl);
            string fileName = Path.Combine(DllsFolderPath, $"Latite.{minecraftVersion}.dll");
            Directory.CreateDirectory(DllsFolderPath);
            File.WriteAllBytes(fileName, data);
            MessageBox.Show($"Latite's DLL for Minecraft {minecraftVersion} has been downloaded!");

            // Check if the checkbox is checked
            if (Checkbox.IsChecked == true)
            {
                MainWindow mainWindow = new();
                mainWindow.LaunchButton_OnRightClick(sender, e);
            }
        }
        catch (WebException ex)
        {
            MessageBox.Show($"Failed to download Latite's DLL for Minecraft {minecraftVersion}. Error: {ex.Message}");
        }
    }
    private string? GetLatestVersion()
    {
        using WebClient webClient = new();
        try
        {
            string latestVersion = webClient.DownloadString(LatestVersionUrl).Trim();
            return latestVersion;
        }
        catch (WebException ex)
        {
            Console.WriteLine($"Failed to retrieve latest version information. Error: {ex.Message}");
            return null;
        }
    }
    private void Onix_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://discord.gg/onixclient");

    private void Latite_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://discord.gg/latite");

    private void Luconia_OnLeftClick(object sender, RoutedEventArgs e) => Process.Start("https://discord.gg/luconia");
    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}