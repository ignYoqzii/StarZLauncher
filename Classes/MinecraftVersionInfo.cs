using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;

namespace StarZLauncher.Classes
{
    public class MinecraftVersionInfo
    {
        // Path to the telemetry_info.json file
        private string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftpe\telemetry_info.json";

        // Version number extracted from the JSON file
        public string VersionNumber { get; private set; }

        // Constructor that reads the JSON file and extracts the version number
        public MinecraftVersionInfo()
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
                // Handle any exceptions that occur while reading the file
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}", "Error");
                // Set a default value for the version number or take other appropriate action
                VersionNumber = "";
            }
        }
    }
}

