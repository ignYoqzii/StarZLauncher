using System;
using System.IO.Compression;
using System.IO;
using System.Windows.Media;
using System.Windows;
using static StarZLauncher.Windows.MainWindow;
using System.Net;
using StarZLauncher.Windows;

namespace StarZLauncher.Classes.Tabs
{
    public static class ToolsManager
    {
        private static readonly string starzScriptsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher\StarZ Scripts\";
        private static readonly string resourcePacksFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs\";
        // Drag and drop code for the textures packs and the lua scripts
        public static void InitializeDragDrop()
        {
            DragAndDropZone!.AllowDrop = true;
            DragAndDropZone.DragEnter += DragZone_DragEnter;
            DragAndDropZone.DragLeave += DragZone_DragLeave;
            DragAndDropZone.Drop += DragZone_Drop;
        }

        private static void DragZone_DragEnter(object sender, DragEventArgs e)
        {
            if (IsLuaFile(e) || IsMcpackOrZipFile(e))
            {
                DragAndDropZone!.BorderThickness = new Thickness(2);
                DragAndDropZone.BorderBrush = Brushes.White;
            }
        }

        private static void DragZone_DragLeave(object sender, DragEventArgs e)
        {
            DragAndDropZone!.BorderThickness = new Thickness(0.5);
            DragAndDropZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
        }

        private static void DragZone_Drop(object sender, DragEventArgs e)
        {
            if (IsLuaFile(e))
            {
                MoveLuaFile(e);
            }
            else if (IsMcpackOrZipFile(e))
            {
                ExtractMcpackOrZipFile(e);
            }

            DragAndDropZone!.BorderThickness = new Thickness(0.5);
            DragAndDropZone.BorderBrush = new SolidColorBrush(Color.FromRgb(85, 170, 255));
        }

        private static bool IsLuaFile(DragEventArgs e)
        {
            return e.Data.GetDataPresent(DataFormats.FileDrop) &&
                   IsFileOfType(((string[])e.Data.GetData(DataFormats.FileDrop))[0], ".lua");
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

        private static void MoveLuaFile(DragEventArgs e)
        {
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (!Directory.Exists(starzScriptsFolder))
            {
                Directory.CreateDirectory(starzScriptsFolder);
            }
            File.Move(filePath, starzScriptsFolder + Path.GetFileName(filePath));
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

        public static void ShaderRemove()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "renderer", "materials");
            string backupPath = Path.Combine(destinationPath, "Backup");

            // Check if StarZ X Minecraft is installed
            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("Make sure the game is installed through the launcher!", "Error !", false);
                return;
            }

            // Check if Backup folder exists
            if (!Directory.Exists(backupPath))
            {
                StarZMessageBox.ShowDialog("No Shaders were previously installed!", "Warning !", false);
                return;
            }

            // Delete 4 files from destination folder
            string[] filesToDelete = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
            };
            foreach (string file in filesToDelete)
            {
                string filePath = Path.Combine(destinationPath, file);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Move 4 files from backup folder to destination folder
            string[] filesToMove = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
            };
            foreach (string file in filesToMove)
            {
                string sourceFilePath = Path.Combine(backupPath, file);
                string destinationFilePath = Path.Combine(destinationPath, file);
                if (File.Exists(sourceFilePath))
                {
                    File.Move(sourceFilePath, destinationFilePath);
                }
            }

            StarZMessageBox.ShowDialog("Shaders removed successfuly!", "Success !", false);
        }

        public static void ShaderApply()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "renderer", "materials");
            string backupPath = Path.Combine(destinationPath, "Backup");

            // Check if destination directory exists
            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("Make sure the game is installed through the launcher!", "Error !", false);
                return;
            }

            // Create backup directory if it doesn't exist
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            if (Directory.GetFiles(backupPath).Length > 0)
            {
                StarZMessageBox.ShowDialog("Shaders are already intsalled!", "Warning !", false);
                return;
            }

            // Move 4 files to backup directory
            string[] filesToMove = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
            };
            foreach (string file in filesToMove)
            {
                string sourceFilePath = Path.Combine(destinationPath, file);
                string destinationFilePath = Path.Combine(backupPath, file);
                if (File.Exists(sourceFilePath))
                {
                    File.Move(sourceFilePath, destinationFilePath);
                }
            }

            // Download 4 files
            string[] filesToDownload = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
            };
            foreach (string file in filesToDownload)
            {
                string url = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/shadersinstaller/{file}";
                string destinationFilePath = Path.Combine(destinationPath, file);
                using WebClient client = new();
                client.DownloadFile(url, destinationFilePath);
            }

            StarZMessageBox.ShowDialog("Shaders applied successfully!", "Success !", false);
        }

        public static void CosmeticsSkinPackApply()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "skin_packs");

            if (!Directory.Exists(destinationPath))
            {
                StarZMessageBox.ShowDialog("Make sure the game is installed trough the launcher!", "Error !", false);
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a persona folder"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sourcePath = dialog.SelectedPath;
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

        public static void SaveProfile()
        {
            RenameWindow renameWindow = new("com.mojang");
            BackgroundForWindowsOnTop!.Visibility = Visibility.Visible;
            bool? result = renameWindow.ShowDialog();

            if (result == true)
            {
                string currentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe", "LocalState", "games", "com.mojang");
                string newFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles", renameWindow.NewNameProfile);

                // Check if the new folder name already exists in the "Profiles" directory
                if (Directory.Exists(newFolderPath))
                {
                    string modifiedNewName = GetUniqueFolderName(renameWindow.NewNameProfile!);
                    newFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles", modifiedNewName);
                }

                // Copy the folder to the new location
                DirectoryCopy(currentFolderPath, newFolderPath);

                StarZMessageBox.ShowDialog("Profile saved successfully!", "Success !", false);
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

        public static void ApplyProfile()
        {
            // Set folder paths
            string gamesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe", "LocalState", "games");
            string commojangFolder = Path.Combine(gamesFolder, "com.mojang");
            string rootFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Profiles");

            // Create and configure the FolderBrowserDialog
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a profile",
                SelectedPath = rootFolderPath
            };

            // Show the FolderBrowserDialog
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Get the selected folder path and name
                string sourcePath = dialog.SelectedPath;
                string sourceFolderName = new DirectoryInfo(sourcePath).Name;

                // Set the destination folder path
                string destinationFolder = Path.Combine(gamesFolder, sourceFolderName);

                // Delete the existing 'com.mojang' folder
                if (Directory.Exists(commojangFolder))
                {
                    Directory.Delete(commojangFolder, true);
                }

                // Copy the selected folder and its elements to the 'games' folder
                DirectoryCopy(sourcePath, destinationFolder);

                // Rename the copied folder to 'com.mojang'
                string renamedFolder = Path.Combine(gamesFolder, "com.mojang");
                Directory.Move(destinationFolder, renamedFolder);
                StarZMessageBox.ShowDialog("Profile applied successfully!", "Success !", false);
            }
        }

    }
}
