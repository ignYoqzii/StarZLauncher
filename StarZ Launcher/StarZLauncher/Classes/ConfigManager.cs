using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StarZLauncher.Classes
{
    // This class handles the Settings.txt file
    public static class ConfigManager
    {
        // Default values for settings
        private static readonly Dictionary<string, object> DefaultSettings = new()
        {
            { "DefaultDLL", "None" },
            { "DiscordRPC", true },
            { "DiscordRPCShowGameVersion", true },
            { "DiscordRPCShowDLLName", true },
            { "Theme", "Light" },
            { "InjectionDelay", "0" },
            { "AccelerateLoadingTime", "0" },
            { "MinecraftInstallationPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Versions") },
            { "OfflineMode", false },
            { "DebugHardwareMonitoring", true },
            { "DebugFontInstaller", false }
        };

        private static readonly string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Settings.txt");
        private static readonly Dictionary<string, object> settings = new(DefaultSettings);

        static ConfigManager()
        {
            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
                File.Create(configFilePath).Close();
                WriteSettingsToFile(DefaultSettings);
            }
            else
            {
                LoadSettingsFromFile();
            }
        }

        private static void WriteSettingsToFile(Dictionary<string, object> settingsToWrite)
        {
            using StreamWriter writer = new(configFilePath);
            foreach (var kvp in settingsToWrite)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }

        private static void LoadSettingsFromFile()
        {
            if (!File.Exists(configFilePath)) return;

            using StreamReader reader = new(configFilePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('=').Select(part => part.Trim()).ToArray();
                if (parts.Length == 2 && settings.ContainsKey(parts[0]))
                {
                    var key = parts[0];
                    var value = parts[1];

                    settings[key] = ConvertValue(settings[key].GetType(), value);
                }
            }
        }

        private static object ConvertValue(Type targetType, string value)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(value);
            }
            if (targetType == typeof(string))
            {
                return value;
            }
            throw new InvalidOperationException($"Unsupported type {targetType}");
        }

        private static void UpdateSetting(string key, object newValue)
        {
            if (settings.ContainsKey(key))
            {
                settings[key] = newValue;
                WriteSettingsToFile(settings);
            }
        }

        public static string GetDefaultDLL() => (string)settings["DefaultDLL"];
        public static bool GetDiscordRPC() => (bool)settings["DiscordRPC"];
        public static bool GetDiscordRPCShowGameVersion() => (bool)settings["DiscordRPCShowGameVersion"];
        public static bool GetDiscordRPCShowDLLName() => (bool)settings["DiscordRPCShowDLLName"];
        public static string GetTheme() => (string)settings["Theme"];
        public static string GetInjectionDelay() => (string)settings["InjectionDelay"];
        public static string GetAccelerateLoadingTime() => (string)settings["AccelerateLoadingTime"];
        public static string GetMinecraftInstallationPath() => (string)settings["MinecraftInstallationPath"];
        public static bool GetOfflineMode() => (bool)settings["OfflineMode"];
        public static bool GetDebugHardwareMonitoring() => (bool)settings["DebugHardwareMonitoring"];
        public static bool GetDebugFontInstaller() => (bool)settings["DebugFontInstaller"];

        public static void SetDefaultDLL(string newDefaultDLL) => UpdateSetting("DefaultDLL", newDefaultDLL);
        public static void SetTheme(string newTheme) => UpdateSetting("Theme", newTheme);
        public static void SetDiscordRPC(bool value) => UpdateSetting("DiscordRPC", value);
        public static void SetDiscordRPCShowGameVersion(bool value) => UpdateSetting("DiscordRPCShowGameVersion", value);
        public static void SetDiscordRPCShowDLLName(bool value) => UpdateSetting("DiscordRPCShowDLLName", value);
        public static void SetInjectionDelay(string newInjectionDelay) => UpdateSetting("InjectionDelay", newInjectionDelay);
        public static void SetAccelerateLoadingTime(string newAccelerateLoadingTime) => UpdateSetting("AccelerateLoadingTime", newAccelerateLoadingTime);
        public static void SetMinecraftInstallationPath(string newMinecraftInstallationPath) => UpdateSetting("MinecraftInstallationPath", newMinecraftInstallationPath);
        public static void SetOfflineMode(bool value) => UpdateSetting("OfflineMode", value);
    }
}