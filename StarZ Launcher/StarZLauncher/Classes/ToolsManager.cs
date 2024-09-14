using StarZLauncher.Windows;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WK.Libraries.BetterFolderBrowserNS;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    /// <summary>
    /// Mostly manages all the buttons and stuff from the Utilities tab
    /// </summary>
    public static class ToolsManager
    {
        private static readonly string resourcePacksFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs\";
        private static readonly string mcOptionsFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftpe\options.txt";
        // Drag and drop code for the textures packs
        public static void InitializeDragDrop()
        {
            DragAndDropZone!.AllowDrop = true;
            DragAndDropZone.DragEnter += DragZone_DragEnter;
            DragAndDropZone.DragLeave += DragZone_DragLeave;
            DragAndDropZone.Drop += DragZone_Drop;
        }

        private static void DragZone_DragEnter(object sender, DragEventArgs e)
        {
            if (IsMcpackOrZipFile(e))
            {
                DragAndDropZone!.BorderThickness = new Thickness(3);
            }
        }

        private static void DragZone_DragLeave(object sender, DragEventArgs e)
        {
            DragAndDropZone!.BorderThickness = new Thickness(0);
        }

        private static void DragZone_Drop(object sender, DragEventArgs e)
        {
            if (IsMcpackOrZipFile(e))
            {
                ExtractMcpackOrZipFile(e);
            }

            DragAndDropZone!.BorderThickness = new Thickness(0);
            DragAndDropZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
        }

        private static bool IsMcpackOrZipFile(DragEventArgs e)
        {
            return e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   (IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".mcpack") ||
                    IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".zip"));
        }

        private static bool IsFileOfType(string filePath, string extension)
        {
            return Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase);
        }

        private static void ExtractMcpackOrZipFile(DragEventArgs e)
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
            catch (Exception)
            {
                StarZMessageBox.ShowDialog("There was a problem extracting your file. It may be corrupted!", "Error !", false);
            }
        }

        private static bool IsMcpackFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".mcpack", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsZipFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task ShaderRemove()
        {
            string? exePath = await GetExecutablePath();
            if (exePath == null) return;

            string destinationPath = Path.Combine(exePath, "data", "renderer", "materials");
            string backupPath = Path.Combine(destinationPath, "Backup");

            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("You are trying to remove shaders from a version of Minecraft that doesn't support them.", "Error!", false);
                return;
            }

            if (!Directory.Exists(backupPath))
            {
                StarZMessageBox.ShowDialog("No Shaders were previously installed!", "Warning!", false);
                return;
            }

            await Task.Run(() =>
            {
                DeleteFiles(destinationPath, new[] { "Sky.material.bin", "Actor.material.bin", "Stars.material.bin", "SunMoon.material.bin" });
                MoveFiles(backupPath, destinationPath, new[] { "Sky.material.bin", "Actor.material.bin", "Stars.material.bin", "SunMoon.material.bin" });
            });

            StarZMessageBox.ShowDialog("Shaders removed successfully!", "Success!", false);
        }

        public static async Task ShaderApply()
        {
            string? exePath = await GetExecutablePath();
            if (exePath == null) return;

            string destinationPath = Path.Combine(exePath, "data", "renderer", "materials");
            string backupPath = Path.Combine(destinationPath, "Backup");

            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("You are trying to apply shaders to an unsupported version of Minecraft.", "Error!", false);
                return;
            }

            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            if (Directory.GetFiles(backupPath).Length > 0)
            {
                StarZMessageBox.ShowDialog("Shaders are already installed!", "Warning!", false);
                return;
            }

            await Task.Run(() =>
            {
                MoveFiles(destinationPath, backupPath, new[] { "Sky.material.bin", "Actor.material.bin", "Stars.material.bin", "SunMoon.material.bin" });
            });

            await DownloadFilesAsync(destinationPath, new[]
            {
                "Sky.material.bin",
                "Actor.material.bin",
                "Stars.material.bin",
                "SunMoon.material.bin"
            });

            StarZMessageBox.ShowDialog("Shaders applied successfully!", "Success!", false);
        }

        private static async Task<string?> GetExecutablePath()
        {
            try
            {
                return await PackageHelper.GetExecutablePath();
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog(ex.Message, "Error !", false);
                return null;
            }
        }

        private static void DeleteFiles(string directoryPath, string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                string filePath = Path.Combine(directoryPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static void MoveFiles(string sourceDirectoryPath, string destinationDirectoryPath, string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                string sourceFilePath = Path.Combine(sourceDirectoryPath, fileName);
                string destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);
                if (File.Exists(sourceFilePath))
                {
                    File.Move(sourceFilePath, destinationFilePath);
                }
            }
        }

        private static async Task DownloadFilesAsync(string destinationPath, string[] fileNames)
        {
            using WebClient client = new();
            foreach (string fileName in fileNames)
            {
                string url = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/shadersinstaller/{fileName}";
                string destinationFilePath = Path.Combine(destinationPath, fileName);
                await client.DownloadFileTaskAsync(new Uri(url), destinationFilePath);
            }
        }

        public static async void CosmeticsSkinPackApply()
        {
            string? exePath;
            try
            {
                exePath = await PackageHelper.GetExecutablePath();
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog(ex.Message, "Error!", false);
                return;
            }
            string destinationPath = Path.Combine(exePath, "data", "skin_packs");

            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("It seems like the folder was deleted or is corrupted. For help, join our Discord server.", "Error !", false);
                return;
            }

            var dialog = new BetterFolderBrowser();
            {
                dialog.Title = "Select a persona folder";
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sourcePath = dialog.SelectedFolder;
                string sourceFolderName = new DirectoryInfo(sourcePath).Name;
                string destinationFolder = Path.Combine(destinationPath, sourceFolderName);

                if (Directory.Exists(destinationFolder))
                {
                    Directory.Delete(destinationFolder, true);
                }

                Directory.Move(sourcePath, destinationFolder);
                StarZMessageBox.ShowDialog("Custom Persona applied successfully!", "Success !", false);
            }
        }

        public static async Task SaveProfile()
        {
            RenameWindow renameWindow = new("com.mojang");
            BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
            bool? result = renameWindow.ShowDialog();

            if (result == true)
            {
                string currentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe", "LocalState", "games", "com.mojang");
                string newFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles", renameWindow.NewNameProfile);

                // Check if the current folder path exists
                if (Directory.Exists(currentFolderPath))
                {
                    // Check if the new folder name already exists in the "Profiles" directory
                    if (Directory.Exists(newFolderPath))
                    {
                        string modifiedNewName = GetUniqueFolderName(renameWindow.NewNameProfile!);
                        newFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles", modifiedNewName);
                    }

                    // Copy the folder to the new location asynchronously
                    await Task.Run(() => DirectoryCopy(currentFolderPath, newFolderPath));

                    StarZMessageBox.ShowDialog("Profile saved successfully!", "Success !", false);
                }
            }

            BackgroundForWindowsOnTop.Visibility = Visibility.Collapsed;
        }

        private static string GetUniqueFolderName(string folderName)
        {
            string modifiedName = folderName;
            int counter = 1;

            // Append a counter to the folder name until it becomes unique
            while (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles", modifiedName)))
            {
                modifiedName = $"{folderName} ({counter})";
                counter++;
            }

            return modifiedName;
        }

        private static void DirectoryCopy(string sourceDirPath, string destDirPath)
        {
            DirectoryInfo sourceDir = new(sourceDirPath);
            DirectoryInfo destDir = new(destDirPath);

            if (!destDir.Exists)
            {
                destDir.Create();
            }

            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string filePath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(filePath, true);
            }

            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string subDirPath = Path.Combine(destDir.FullName, subDir.Name);
                DirectoryCopy(subDir.FullName, subDirPath);
            }
        }

        public static async Task ApplyProfile()
        {
            // Set folder paths
            string gamesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe", "LocalState", "games");
            string commojangFolder = Path.Combine(gamesFolder, "com.mojang");
            string rootFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles");

            // Create and configure the FolderBrowserDialog
            var dialog = new BetterFolderBrowser();
            {
                dialog.Title = "Select a profile";
                dialog.RootFolder = rootFolderPath;
            };

            // Show the FolderBrowserDialog
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Get the selected folder path and name
                string sourcePath = dialog.SelectedFolder;
                string sourceFolderName = new DirectoryInfo(sourcePath).Name;

                // Set the destination folder path
                string destinationFolder = Path.Combine(gamesFolder, sourceFolderName);

                // Delete the existing 'com.mojang' folder
                if (Directory.Exists(commojangFolder))
                {
                    await Task.Run(() => Directory.Delete(commojangFolder, true));
                }

                // Copy the selected folder and its elements to the 'games' folder
                await Task.Run(() => DirectoryCopy(sourcePath, destinationFolder));

                // Rename the copied folder to 'com.mojang'
                string renamedFolder = Path.Combine(gamesFolder, "com.mojang");
                await Task.Run(() => Directory.Move(destinationFolder, renamedFolder));

                StarZMessageBox.ShowDialog("Profile applied successfully!", "Success !", false);
            }
        }

        public static void VSyncEnable()
        {
            // Check if the Minecraft options file exists
            if (!File.Exists(mcOptionsFile))
            {
                StarZMessageBox.ShowDialog("Make sure Minecraft is installed. If it is the case, launch the game one time to make sure all the config files are correctly created.", "Error !", false);
                return;
            }

            // Read all lines from the Minecraft options file
            string[] lines = File.ReadAllLines(mcOptionsFile);

            // Find the line that starts with "gfx_vsync:"
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("gfx_vsync:"))
                {
                    // Replace the value with "1"
                    lines[i] = "gfx_vsync:1";
                    break; // Stop searching once found
                }
            }

            // Write the modified lines back to the Minecraft options file
            File.WriteAllLines(mcOptionsFile, lines);

            StarZMessageBox.ShowDialog("V-Sync is now enabled!", "Success !", false);
        }

        public static void VSyncDisable()
        {
            // Check if the Minecraft options file exists
            if (!File.Exists(mcOptionsFile))
            {
                StarZMessageBox.ShowDialog("Make sure Minecraft is installed. If it is the case, launch the game one time to make sure all the config files are correctly created.", "Error !", false);
                return;
            }

            // Read all lines from the Minecraft options file
            string[] lines = File.ReadAllLines(mcOptionsFile);

            // Find the line that starts with "gfx_vsync:"
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("gfx_vsync:"))
                {
                    // Replace the value with "0"
                    lines[i] = "gfx_vsync:0";
                    break; // Stop searching once found
                }
            }

            // Write the modified lines back to the Minecraft options file
            File.WriteAllLines(mcOptionsFile, lines);

            StarZMessageBox.ShowDialog("V-Sync is now disabled!", "Success !", false);
        }
    }
}