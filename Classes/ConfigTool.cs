using System;
using System.IO;
using System.Collections.Generic;

namespace StarZLauncher
{
    public static class ConfigTool
    {
        // Default values for settings
        private const string DEFAULT_DLL = "None";
        private const bool DEFAULT_DISCORD_RPC = true;
        private const bool DEFAULT_DISCORD_RPC_SHOW_GAME_VERSION = true;
        private const bool DEFAULT_DISCORD_RPC_SHOW_DLL_NAME = true;
        private const string DEFAULT_LAUNCHER_COLOR = "Default";
        private const string DEFAULT_LAUNCHER_BACKGROUND = "Default";
        private const string LAUNCH_OPTION = "RemainOpen";

        private static readonly string configFilePath;

        // Settings and their values
        private static readonly Dictionary<string, object> settings = new();

        static ConfigTool()
        {
            // Set default values for settings
            settings.Add("DefaultDLL", DEFAULT_DLL);
            settings.Add("DiscordRPC", DEFAULT_DISCORD_RPC);
            settings.Add("DiscordRPCShowGameVersion", DEFAULT_DISCORD_RPC_SHOW_GAME_VERSION);
            settings.Add("DiscordRPCShowDLLName", DEFAULT_DISCORD_RPC_SHOW_DLL_NAME);
            settings.Add("LauncherColor", DEFAULT_LAUNCHER_COLOR);
            settings.Add("LauncherBackground", DEFAULT_LAUNCHER_BACKGROUND);
            settings.Add("LaunchOption", LAUNCH_OPTION);

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

        public static string GetLauncherColor()
        {
            return (string)settings["LauncherColor"];
        }

        public static string GetLauncherBackground()
        {
            return (string)settings["LauncherBackground"];
        }

        public static string GetLaunchOption()
        {
            return (string)settings["LaunchOption"];
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

        public static void SetLauncherBackground(string newLauncherBackground)
        {
            // Update the value in the settings dictionary
            settings["LauncherBackground"] = newLauncherBackground;

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

        public static void SetLaunchOption(string newLaunchOption)
        {
            // Update the value in the settings dictionary
            settings["LaunchOption"] = newLaunchOption;

            // Write the updated settings to the config file
            using StreamWriter writer = new(configFilePath);
            foreach (KeyValuePair<string, object> kvp in settings)
            {
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }
    }
}

