using Microsoft.Win32;
using StarZLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;
using static StarZLauncher.Windows.MainWindow;
using System.Linq;

namespace StarZLauncher.Classes
{
    public static class ModsManager
    {
        private static readonly string MOD_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Mods");
        private static readonly string JSON_FILE = Path.Combine(MOD_FOLDER, "ModsCollection.json");
        public static ObservableCollection<ModInfo> ModItems { get; private set; } = new(); // To display in the launcher

        public static string? DefaultMod { get; private set; }
        private static List<ModInfo> ModInfos = new(); // For the JSON file

        public static void LoadDefaultMod()
        {
            DefaultMod = ConfigManager.GetDefaultMod();
            DefaultDLLText!.Content = $"Default Mod on launch: {DefaultMod}";
        }

        public static void LoadMods()
        {
            ModInfos = File.Exists(JSON_FILE)
                ? JsonSerializer.Deserialize<List<ModInfo>>(File.ReadAllText(JSON_FILE)) ?? new List<ModInfo>()
                : new List<ModInfo>();

            ModItems.Clear();
            foreach (var modInfo in ModInfos)
            {
                ModItems.Add(modInfo);
            }
        }

        public static void SaveMods()
        {
            Directory.CreateDirectory(MOD_FOLDER);
            File.WriteAllText(JSON_FILE, JsonSerializer.Serialize(ModInfos));
        }

        public static void SetDefaultMod()
        {
            if (ModsListManager?.SelectedItem is not ModInfo selectedItem) return;

            if (StarZMessageBox.ShowDialog($"Set the default Mod to {selectedItem.Name}? It will automatically use this Mod when launching.", "Warning!") != true) return;

            ConfigManager.SetDefaultMod(selectedItem.Name);
            LoadDefaultMod();
        }

        public static void Edit()
        {
            if (ModsListManager?.SelectedItem is not ModInfo selectedMod) return;

            bool isEnabled = selectedMod.Type == "web_dll" || selectedMod.Type == "web_exe";

            var editWindow = new EditWindow(Path.GetFileNameWithoutExtension(selectedMod.Name), selectedMod.Url, isEnabled);
            BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;

            if (editWindow.ShowDialog() == true)
            {
                var newName = editWindow.NewName?.Trim();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    // Determine the correct file extension based on the type
                    string newExtension = selectedMod.Type switch
                    {
                        "web_dll" => ".dll",
                        "web_exe" => ".exe",
                        _ => Path.GetExtension(selectedMod.Name) // Keep the original extension if type is not web_dll or web_exe
                    };

                    var newFullName = newName + newExtension;

                    if (newFullName != selectedMod.Name && !File.Exists(Path.Combine(MOD_FOLDER, newFullName)))
                    {
                        File.Move(Path.Combine(MOD_FOLDER, selectedMod.Name), Path.Combine(MOD_FOLDER, newFullName));

                        if (selectedMod.Name == DefaultMod)
                        {
                            ConfigManager.SetDefaultMod(newFullName);
                            LoadDefaultMod();
                        }

                        selectedMod.Name = newFullName;
                    }
                }

                if (selectedMod.Type == "web_dll" || selectedMod.Type == "web_exe")
                {
                    if (!string.IsNullOrWhiteSpace(editWindow.NewURL))
                    {
                        selectedMod.Url = editWindow.NewURL!.Trim();
                    }
                }

                SaveMods();
                LoadMods();
            }

            BackgroundForWindowsOnTop!.Visibility = Visibility.Collapsed;
        }

        public static void Reset()
        {
            if (StarZMessageBox.ShowDialog("Reset the default Mod to NONE?", "Warning!") != true) return;

            ConfigManager.SetDefaultMod("None");
            LoadDefaultMod();
        }

        public static void Delete()
        {
            if (ModsListManager?.SelectedItem is not ModInfo selectedMod) return;

            if (StarZMessageBox.ShowDialog("Delete the selected Mod? If it's the default, it will be reset to NONE.", "Warning!") != true) return;

            var filePath = Path.Combine(MOD_FOLDER, selectedMod.Name);
            if (File.Exists(filePath)) File.Delete(filePath);

            ModInfos.Remove(selectedMod);
            ModItems.Remove(selectedMod);

            if (selectedMod.Name == DefaultMod)
            {
                ConfigManager.SetDefaultMod("None");
                LoadDefaultMod();
            }

            SaveMods();
            LoadMods();
        }

        public static void Add()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|EXE files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select a DLL or EXE file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = Path.GetFileName(openFileDialog.FileName);
                if (ModInfos.Exists(mod => mod.Name == fileName))
                {
                    StarZMessageBox.ShowDialog("A Mod with the same name already exists.", "Error!");
                    return;
                }

                var extension = Path.GetExtension(fileName).ToLower();
                if (extension == ".exe" || extension == ".dll")
                {
                    // Determine the mod type based on file extension
                    var modType = extension == ".exe" ? "local_exe" : "local_dll";

                    var destinationFilePath = Path.Combine(MOD_FOLDER, fileName);
                    Directory.CreateDirectory(MOD_FOLDER);
                    File.Copy(openFileDialog.FileName, destinationFilePath);

                    var newMod = new ModInfo { Name = fileName, Type = modType, Url = null };
                    ModInfos.Add(newMod);
                    ModItems.Add(newMod);
                    SaveMods();
                }
                else
                {
                    StarZMessageBox.ShowDialog("Please select a valid .exe or .dll file.", "Invalid File Type");
                }
            }
        }

        public static async Task AddFromWeb(string url)
        {
            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            if (ModInfos.Exists(mod => mod.Name == fileName))
            {
                StarZMessageBox.ShowDialog("A Mod with the same name already exists.", "Error!");
                return;
            }

            var destinationFilePath = Path.Combine(MOD_FOLDER, fileName);
            Directory.CreateDirectory(MOD_FOLDER);

            try
            {
                using var client = new System.Net.Http.HttpClient();

                // Get the file bytes
                var fileBytes = await client.GetByteArrayAsync(url);

                // Use the extension from the URL (or file name) to determine if it's an exe or dll
                var extension = Path.GetExtension(fileName).ToLower();

                // If the content type contains 'exe' or 'dll', use that as a fallback check
                if (extension == ".exe" || extension == ".dll")
                {
                    // Save the file
                    File.WriteAllBytes(destinationFilePath, fileBytes);

                    // Set the mod type based on the extension
                    var modType = extension == ".exe" ? "web_exe" : "web_dll";

                    var newMod = new ModInfo { Name = fileName, Type = modType, Url = url };
                    ModInfos.Add(newMod);
                    ModItems.Add(newMod);
                    SaveMods();
                }
                else
                {
                    StarZMessageBox.ShowDialog("The downloaded file is neither a .dll nor a .exe.", "Invalid File Type");
                }
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Error downloading file: {ex.Message}", "Error!");
            }
        }


        public static string GetDefaultModType()
        {
            if (string.IsNullOrEmpty(DefaultMod)) return "None";

            // Find the ModInfo of the Default Mod
            var defaultModInfo = ModInfos.FirstOrDefault(mod => mod.Name == DefaultMod);
            if (defaultModInfo == null) return "None";

            // Check the file extension to determine if it's a DLL or EXE
            string fileExtension = Path.GetExtension(defaultModInfo.Name).ToLower();

            if (fileExtension == ".dll")
                return "DLL";
            else if (fileExtension == ".exe")
                return "EXE";
            else
                return "Unknown";
        }

        // Method to check if DefaultMod is web or local, and if web, compare the hash value of the file at the URL with the local file.
        public static async Task VerifyAndUpdateModHash()
        {
            if (string.IsNullOrEmpty(DefaultMod)) return;

            // Find the ModInfo of the DefaultMod
            var defaultModInfo = ModInfos.FirstOrDefault(mod => mod.Name == DefaultMod);
            if (defaultModInfo == null) return;

            // Check if it's a web DLL
            if (defaultModInfo.Type == "web_dll" || defaultModInfo.Type == "web_exe")
            {
                if (!string.IsNullOrEmpty(defaultModInfo.Url))
                {
                    var localFilePath = Path.Combine(MOD_FOLDER, defaultModInfo.Name);

                    if (File.Exists(localFilePath))
                    {
                        // Get the MD5 hash of the local file
                        string localFileHash = GetFileHash(localFilePath);

                        try
                        {
                            using var client = new System.Net.Http.HttpClient();
                            var fileBytes = await client.GetByteArrayAsync(defaultModInfo.Url);
                            string webFileHash = GetHashFromBytes(fileBytes);

                            // Check if the local file's hash matches the downloaded file's hash
                            if (localFileHash != webFileHash)
                            {
                                // File is different, so redownload it (update the local file)
                                UpdateLocalFileWithWebFile(fileBytes, localFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            StarZMessageBox.ShowDialog($"Error downloading or comparing the file: {ex.Message}", "Error!");
                        }
                    }
                    else
                    {
                        StarZMessageBox.ShowDialog("The local file does not exist.", "File Missing!");
                    }
                }
                else
                {
                    StarZMessageBox.ShowDialog("The URL to the Web-based Mod is missing. Delete the file and add it again from the URL.", "URL Missing!");
                }
            }
        }

        // Helper method to get the MD5 hash of a local file
        private static string GetFileHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var fileStream = File.OpenRead(filePath);
            byte[] hashBytes = md5.ComputeHash(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        // Helper method to get the MD5 hash from a byte array (for the web file)
        private static string GetHashFromBytes(byte[] fileBytes)
        {
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(fileBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        // Helper method to update the local file with the downloaded content
        private static void UpdateLocalFileWithWebFile(byte[] fileBytes, string localFilePath)
        {
            // Write the file bytes to the local path
            try
            {
                // Make sure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

                // Overwrite the local file with the new content
                File.WriteAllBytes(localFilePath, fileBytes);
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Error updating the file: {ex.Message}", "Error!");
            }
        }

        public class ModInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty; // "local_dll", "web_dll", "local_exe", "web_exe"
            public string? Url { get; set; } = string.Empty; // URL for Web-based DLLs or EXEs

            public string DisplayTypeText => Type switch
            {
                "web_dll" => "Web-based DLL Client / Mod (Supports automatic updates)",
                "local_dll" => "Local DLL Client / Mod",
                "web_exe" => "Web-based EXE Client / Mod (Supports automatic updates)",
                "local_exe" => "Local EXE Client / Mod",
                _ => "Unknown Mod Type"
            };

            public string ImageName => Type switch
            {
                "web_dll" => "/Resources/WebMod.png",
                "local_dll" => "/Resources/DLL.png",
                "web_exe" => "/Resources/WebMod.png",
                "local_exe" => "/Resources/EXE.png",
                _ => "/Resources/Unknown.png"
            };
        }
    }
}