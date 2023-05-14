using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using static StarZLauncher.Windows.MainWindow;
using static StarZLauncher.Classes.Tabs.DLLsManager;
using StarZLauncher.Classes.Settings;
using StarZLauncher.Classes.Discord;

namespace StarZLauncher.Classes.Game
{
    public static class Launch
    {
        public static Process? Minecraft;
        public static bool IsMinecraftRunning;
        private static readonly string DllsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "DLLs");
        public static string? DllNameLaunchOnLeftClick { get; private set; }
        public static string? DllNameLaunchOnRightClick { get; private set; }
        readonly static string? versionNumber = VersionHelper.VersionNumber;
        static Launch()
        {
        }

        // Load the default dll name on launch to display on the mainwindow
        private static void LoadDefaultDLL()
        {
            defaultDll = ConfigTool.GetDefaultDLL();
        }

        public static void OpenGame()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = "explorer.exe",
                    Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App",
                    UseShellExecute = true,
                }
            };

            process.Start();

            Task.Run(() =>
            {
                Task.Delay(1500).Wait();
                LoadGameFaster();
            });
        }

        private static void LoadGameFaster()
        {
            var brokers = Process.GetProcessesByName("RuntimeBroker");
            if (brokers.Length <= 0) return;

            foreach (var broker in brokers)
            {
                broker.Kill();
            }
        }

        // handle the game closing event
        private static void IfMinecraftExited(object sender, EventArgs e)
        {
            bool DiscordRPCisEnabled = ConfigTool.GetDiscordRPC();
            if (DiscordRPCisEnabled == true)
            {
                DiscordRichPresenceManager.IdlePresence();
            }
            IsMinecraftRunning = false;
        }

        public async static void LaunchGameOnRightClick()
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

            if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

            OpenGame();

            while (true)
            {
                if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
                Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                break;
            }

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
                    mainWindow?.MinimizeToTrayWindow();
                }
                else if (LaunchOptionisSelected == "Minimize")
                {
                    mainWindow?.MinimizeWindow();
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

        public async static void LaunchGameOnLeftClick()
        {
            if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

            OpenGame();

            while (true)
            {
                if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
                Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                break;
            }

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
                                mainWindow?.MinimizeToTrayWindow();
                            }
                            else if (LaunchOptionisSelected == "Minimize")
                            {
                                mainWindow?.MinimizeWindow();
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
    }
}
