﻿using Microsoft.Win32;
using StarZLauncher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace StarZLauncher.Classes
{
    public static class Launch
    {
        public static Process? Minecraft;
        private static readonly string ModsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Mods");
        public static string? DllNameLaunchOnRightClick { get; private set; }

        // Check if Minecraft is opened
        public static bool IsMinecraftOpened => (Minecraft = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault()) != null;

        // Launch Minecraft if it's not already opened
        public static async Task OpenGame()
        {
            if (!IsMinecraftOpened)
            {
                var MinecraftApplication = await PackageHelper.GetPackage() ?? throw new Exception("Minecraft could not be launched. This is likely due to it not being installed or being corrupted.");
                await MinecraftApplication.LaunchAsync();
                Minecraft = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();

                MinimizeFix();
                await LoadGameFaster();
            }
        }

        // Accelerate game loading by killing RuntimeBroker processes
        private static async Task LoadGameFaster()
        {
            if (!int.TryParse(ConfigManager.GetAccelerateLoadingTime(), out int delayMilliseconds) || delayMilliseconds == 0) return;

            await Task.Delay(delayMilliseconds);

            foreach (var broker in Process.GetProcessesByName("RuntimeBroker"))
            {
                try { broker.Kill(); }
                catch (Exception ex) { StarZMessageBox.ShowDialog(ex.Message, "Error!", false); }
            }
        }

        // Fix for minimizing the game
        private static void MinimizeFix()
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "AppLifecycleOptOut.exe");
            string packageName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

            if (File.Exists(exePath))
            {
               Process.Start(exePath, $"\"{exePath}\" \"{packageName}\"");
            }
        }

        // Handle the game closing event
        private static void IfMinecraftExited(object sender, EventArgs e)
        {
            if (ConfigManager.GetDiscordRPC() && !ConfigManager.GetOfflineMode())
            {
                string state = ConfigManager.GetDiscordRPCIdleStatus();
                DiscordRichPresenceManager.IdlePresence(state);
            }
        }

        // Launch game and inject DLL on right-click
        public async static void LaunchGameOnRightClick()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                RestoreDirectory = true,
                InitialDirectory = ModsFolderPath
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                await OpenGame();
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"{ex.Message}", "Error!", false);
                return;
            }
            await InjectAndHandlePresence(openFileDialog.FileName);
        }

        // Launch game and inject default DLL on left-click
        public async static void LaunchGameOnLeftClick()
        {
            try
            {
                await OpenGame();
            }
            catch (Exception ex) 
            {
                StarZMessageBox.ShowDialog($"{ex.Message}", "Error!", false);
                return;
            }

            string DefaultModName = ModsManager.DefaultMod!;

            if (string.IsNullOrEmpty(DefaultModName) || DefaultModName == "None")
            {
                Minecraft!.EnableRaisingEvents = true;
                Minecraft.Exited += IfMinecraftExited;

                bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
                bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
                bool DiscordShowGameVersionisEnabled = ConfigManager.GetDiscordRPCShowGameVersion();

                if (!DiscordRPCisEnabled || OfflineModeisEnabled) return;

                DiscordRichPresenceManager.DiscordClient.UpdateDetails(DiscordShowGameVersionisEnabled ? $"Playing Minecraft {VersionHelper.VersionNumber}" : "Playing Minecraft");
                DiscordRichPresenceManager.DiscordClient.UpdateState("");

                return;
            }

            string ModFilePath = Path.Combine(ModsFolderPath, DefaultModName);

            if (File.Exists(ModFilePath))
            {
                await ModsManager.VerifyAndUpdateModHash();

                var result = ModsManager.GetDefaultModType();
                if (result == "DLL")
                {
                    await InjectAndHandlePresence(ModFilePath);
                }
                else if (result == "EXE")
                {
                    if (int.TryParse(ConfigManager.GetInjectionDelay(), out int delayMilliseconds))
                    {
                        await Task.Delay(delayMilliseconds);
                    }
                    Process.Start(ModFilePath);
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(ModFilePath);
                    SetDiscordPresence(filenameWithoutExtension);
                    Minecraft!.EnableRaisingEvents = true;
                    Minecraft.Exited += IfMinecraftExited;
                }
                else
                {
                    // Should not happen
                    return;
                }
            }
            else
            {
                StarZMessageBox.ShowDialog("The DLL you provided couldn't be found!", "Error !", false);
            }
        }

        // Common method to inject DLL and handle Discord presence
        private static async Task InjectAndHandlePresence(string dllPath)
        {
            try
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);
                SetDiscordPresence(filenameWithoutExtension);

                if (int.TryParse(ConfigManager.GetInjectionDelay(), out int delayMilliseconds))
                {
                    await Task.Delay(delayMilliseconds);
                    await Injector.Inject(dllPath);
                }

                Minecraft!.EnableRaisingEvents = true;
                Minecraft.Exited += IfMinecraftExited;
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Injection failed! {ex.Message}", "Error!", false);
            }
        }

        // Handle Discord Rich Presence updates
        private static void SetDiscordPresence(string ModName)
        {
            bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
            bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
            bool DiscordShowGameVersionisEnabled = ConfigManager.GetDiscordRPCShowGameVersion();
            bool DiscordShowDLLNameisEnabled = ConfigManager.GetDiscordRPCShowDLLName();

            if (!DiscordRPCisEnabled || OfflineModeisEnabled) return;

            DiscordRichPresenceManager.DiscordClient.UpdateDetails(DiscordShowGameVersionisEnabled ? $"Playing Minecraft {VersionHelper.VersionNumber}" : "Playing Minecraft");
            DiscordRichPresenceManager.DiscordClient.UpdateState(DiscordShowDLLNameisEnabled ? $"With {ModName}" : "");
        }
    }
}