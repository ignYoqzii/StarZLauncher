using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace StarZLauncher.Classes
{
    public class VersionInfo
    {
        // Path to the telemetry_info.json file
        public string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftpe\telemetry_info.json";

        // Version number extracted from the JSON file
        public string VersionNumber { get; private set; }
        public string LauncherVersion { get; private set; }

        // Constructor that reads the JSON file and extracts the version number
        public VersionInfo()
        {
            try
            {
                // Read the JSON file as a string
                string json = System.IO.File.ReadAllText(filePath);

                // Parse the JSON string into a JObject
                JObject jObject = JObject.Parse(json);

                // Extract the version number from the "lastsession_Build" property
                VersionNumber = (string)jObject["lastsession_Build"];
            }
            catch (Exception ex)
            {
                // Set a default value for the version number or take other appropriate action
                VersionNumber = "Error";
            }

            string url = "https://raw.githubusercontent.com/ignYoqzii/StarZLauncher/main/LauncherVersion.txt";

            try
            {
                WebClient client = new WebClient();
                LauncherVersion = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                LauncherVersion = "Error";
            }
        }
    }
}

