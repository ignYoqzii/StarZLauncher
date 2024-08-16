using System;
using System.Collections.Generic;
using System.IO;

namespace StarZLauncher.Classes
{
    // This class handles the Settings.txt file
    public static class ConfigManager
    {
        // Default values for settings
        private const string DEFAULT_DLL = "None";
        private const bool DEFAULT_DISCORD_RPC = true;
        private const bool DEFAULT_DISCORD_RPC_SHOW_GAME_VERSION = true;
        private const bool DEFAULT_DISCORD_RPC_SHOW_DLL_NAME = true;
        private const string THEME = "Light";
        private const string InjectionDelay = "0";
        private const string AccelerateLoadingTime = "0";
        private static string MINECRAFTINSTALLATIONPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Versions");
        private static bool OFFLINEMODE = false;
        private static bool DEBUGHARDWAREMONITORING = true;
        private static bool DEBUGFONTINSTALLER = false;

        private static readonly string configFilePath;

        // Settings and their values
        private static readonly Dictionary<string, object> settings = new();

        static ConfigManager()
        {
            // Set default values for settings
            settings.Add("DefaultDLL", DEFAULT_DLL);
            settings.Add("DiscordRPC", DEFAULT_DISCORD_RPC);
            settings.Add("DiscordRPCShowGameVersion", DEFAULT_DISCORD_RPC_SHOW_GAME_VERSION);
            settings.Add("DiscordRPCShowDLLName", DEFAULT_DISCORD_RPC_SHOW_DLL_NAME);
            settings.Add("Theme", THEME);
            settings.Add("InjectionDelay", InjectionDelay);
            settings.Add("AccelerateLoadingTime", AccelerateLoadingTime);
            settings.Add("MinecraftInstallationPath", MINECRAFTINSTALLATIONPATH);
            settings.Add("OfflineMode", OFFLINEMODE);
            settings.Add("DebugHardwareMonitoring", DEBUGHARDWAREMONITORING);
            settings.Add("DebugFontInstaller", DEBUGFONTINSTALLER);

            // Get the config file path in MyDocuments/StarZ Launcher
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            configFilePath = Path.Combine(myDocumentsPath, "StarZ Launcher", "Settings.txt");

            // Create the config file if it doesn't already exist
            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
                File.Create(configFilePath).Close();
                WriteDefaultSettingsToFile();
            }

            // Wait for a short time to give the file creation process a chance to complete
            System.Threading.Thread.Sleep(100);

            // Load settings from the config file
            LoadSettings();
        }

        private static void WriteDefaultSettingsToFile()
        {
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        private static void LoadSettings()
        {
            // Read the config file line by line
            using StreamReader reader = new(configFilePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Split each line into key and value
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    // Trim whitespace from key and value
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    // Convert value to correct type and add to settings
                    if (settings.ContainsKey(key))
                    {
                        if (settings[key] is bool)
                        {
                            settings[key] = bool.Parse(value);
                        }
                        else if (settings[key] is string)
                        {
                            settings[key] = value;
                        }
                    }
                }
            }
        }

        public static string GetDefaultDLL()
        {
            return (string)settings["DefaultDLL"];
        }

        public static bool GetDiscordRPC()
        {
            return (bool)settings["DiscordRPC"];
        }

        public static bool GetDiscordRPCShowGameVersion()
        {
            return (bool)settings["DiscordRPCShowGameVersion"];
        }

        public static bool GetDiscordRPCShowDLLName()
        {
            return (bool)settings["DiscordRPCShowDLLName"];
        }

        public static string GetTheme()
        {
            return (string)settings["Theme"];
        }

        public static string GetInjectionDelay()
        {
            return (string)settings["InjectionDelay"];
        }

        public static string GetAccelerateLoadingTime()
        {
            return (string)settings["AccelerateLoadingTime"];
        }

        public static string GetMinecraftInstallationPath()
        {
            return (string)settings["MinecraftInstallationPath"];
        }

        public static bool GetOfflineMode()
        {
            return (bool)settings["OfflineMode"];
        }

        // This is only for debugging purpose
        public static bool GetDebugHardwareMonitoring()
        {
            return (bool)settings["DebugHardwareMonitoring"];
        }

        public static bool GetDebugFontInstaller()
        {
            return (bool)settings["DebugFontInstaller"];
        }

        public static void SetDefaultDLL(string newDefaultDLL)
        {
            // Update the value in the settings dictionary
            settings["DefaultDLL"] = newDefaultDLL;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetTheme(string newTheme)
        {
            // Update the value in the settings dictionary
            settings["Theme"] = newTheme;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetDiscordRPC(bool value)
        {
            // Update the value in the settings dictionary
            settings["DiscordRPC"] = value;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetDiscordRPCShowGameVersion(bool value)
        {
            // Update the value in the settings dictionary
            settings["DiscordRPCShowGameVersion"] = value;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetDiscordRPCShowDLLName(bool value)
        {
            // Update the value in the settings dictionary
            settings["DiscordRPCShowDLLName"] = value;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetInjectionDelay(string newInjectionDelay)
        {
            // Update the value in the settings dictionary
            settings["InjectionDelay"] = newInjectionDelay;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetAccelerateLoadingTime(string newAccelerateLoadingTime)
        {
            // Update the value in the settings dictionary
            settings["AccelerateLoadingTime"] = newAccelerateLoadingTime;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetMinecraftInstallationPath(string newMinecraftInstallationPath)
        {
            // Update the value in the settings dictionary
            settings["MinecraftInstallationPath"] = newMinecraftInstallationPath;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        public static void SetOfflineMode(bool value)
        {
            // Update the value in the settings dictionary
            settings["OfflineMode"] = value;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }
    }
}

