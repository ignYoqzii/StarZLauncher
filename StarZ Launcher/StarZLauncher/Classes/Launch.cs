using Microsoft.Win32;
using StarZLauncher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static StarZLauncher.Classes.DLLsManager;
using static System.WindowsRuntimeSystemExtensions;

namespace StarZLauncher.Classes
{
    /// <summary>
    /// Class used to launch Minecraft and manage everything related to the game launching event.
    /// </summary>
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
            defaultDll = ConfigManager.GetDefaultDLL();
        }

        public static void OpenGame()
        {
            Process.Start("minecraft:");
            MinimizeFix();
            LoadGameFaster();
        }

        // Fix to make the game launch faster
        private async static void LoadGameFaster()
        {
            string AccelerateLoadingTimeValue = ConfigManager.GetAccelerateLoadingTime();
            if (AccelerateLoadingTimeValue == "0")
            {
                return;
            }
            else if (int.TryParse(AccelerateLoadingTimeValue, out int delayMilliseconds))
            {
                // Add the delay using the parsed value
                await Task.Delay(delayMilliseconds);

                var brokers = Process.GetProcessesByName("RuntimeBroker");
                if (brokers.Length <= 0) return;

                foreach (var broker in brokers)
                {
                    try
                    {
                        // Attempt to kill the process
                        broker.Kill();
                    }
                    catch (Exception ex)
                    {
                        StarZMessageBox.ShowDialog($"{ex.Message}", "Error !", false);
                    }
                }
            }
        }

        // I was to lazy to make the code myself, as I don't really know how to code it properly.
        static void MinimizeFix()
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "AppLifecycleOptOut.exe");
            string packageName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

            if (File.Exists(exePath))
            {
                string arguments = $"\"{exePath}\" \"{packageName}\"";
                Process.Start(exePath, arguments);
            }
        }

        // Handle the game closing event
        private static void IfMinecraftExited(object sender, EventArgs e)
        {
            bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
            bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
            if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
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

            if (Process.GetProcessesByName("Minecraft.Windows").Length != 0) return;

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
                bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
                bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
                bool DiscordShowGameVersionisEnabled = ConfigManager.GetDiscordRPCShowGameVersion();
                bool DiscordShowDLLNameisEnabled = ConfigManager.GetDiscordRPCShowDLLName();
                if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
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

                string InjectionDelayValue = ConfigManager.GetInjectionDelay();

                if (int.TryParse(InjectionDelayValue, out int delayMilliseconds))
                {
                    // Add the delay using the parsed value
                    await Task.Delay(delayMilliseconds);

                    await Injector.Inject(openFileDialog.FileName);
                }
                IsMinecraftRunning = true;
                Minecraft.EnableRaisingEvents = true;
                Minecraft.Exited += IfMinecraftExited;
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Injection failed! {ex.Message}", "Error !", false);
            }
        }

        public async static void LaunchGameOnLeftClick()
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length != 0) return;

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
                            bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
                            bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
                            bool DiscordShowGameVersionisEnabled = ConfigManager.GetDiscordRPCShowGameVersion();
                            bool DiscordShowDLLNameisEnabled = ConfigManager.GetDiscordRPCShowDLLName();
                            if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
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

                            string InjectionDelayValue = ConfigManager.GetInjectionDelay();

                            if (int.TryParse(InjectionDelayValue, out int delayMilliseconds))
                            {
                                // Add the delay using the parsed value
                                await Task.Delay(delayMilliseconds);

                                await Injector.Inject(dllFilePath);
                            }
                            IsMinecraftRunning = true;
                            Minecraft.EnableRaisingEvents = true;
                            Minecraft.Exited += IfMinecraftExited;
                        }
                        catch (Exception ex)
                        {
                            StarZMessageBox.ShowDialog($"Injection failed! {ex.Message}", "Error !", false);
                        }
                    }
                    else
                    {
                        StarZMessageBox.ShowDialog("The DLL you provided couldn't be found!", "Error !", false);
                    }
                }
                else
                {
                    StarZMessageBox.ShowDialog($"Settings.txt error on line 1 of the file.", "Error !", false);
                }
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Failed to read Settings.txt. {ex.Message}", "Error !", false);
            }
        }
    }
}
