using System;
using System.IO.Compression;
using System.IO;
using System.Windows.Media;
using System.Windows;
using static StarZLauncher.Windows.MainWindow;
using System.Net;

namespace StarZLauncher.Classes.Tabs
{
    public static class ToolsManager
    {
        private static readonly string starzScriptsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\StarZ Launcher\StarZ Scripts\";
        private static readonly string resourcePacksFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\resource_packs\";
        // Drag and drop code for the textures packs and the lua scripts
        public static void InitializeDragDrop()
        {
            DragAndDropZone.AllowDrop = true;
            DragAndDropZone.DragEnter += DragZone_DragEnter;
            DragAndDropZone.DragLeave += DragZone_DragLeave;
            DragAndDropZone.Drop += DragZone_Drop;
        }

        private static void DragZone_DragEnter(object sender, DragEventArgs e)
        {
            if (IsLuaFile(e) || IsMcpackOrZipFile(e))
            {
                DragAndDropZone.BorderThickness = new Thickness(2);
                DragAndDropZone.BorderBrush = Brushes.White;
            }
        }

        private static void DragZone_DragLeave(object sender, DragEventArgs e)
        {
            DragAndDropZone.BorderThickness = new Thickness(0.5);
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

            DragAndDropZone.BorderThickness = new Thickness(0.5);
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
            catch (Exception ex)
            {
                MessageBox.Show("Error extracting file: " + ex.Message);
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
                MessageBox.Show("The game needs to be installed through the launcher !");
                return;
            }

            // Check if Backup folder exists
            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show("No shaders previously installed.");
                return;
            }

            // Delete 4 files from destination folder
            string[] filesToDelete = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
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
        "Sky.material.bin",
        "SunMoon.material.bin"
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

            MessageBox.Show("Shaders removed successfully!");
        }

        public static void ShaderApply()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "renderer", "materials");
            string backupPath = Path.Combine(destinationPath, "Backup");

            // Check if destination directory exists
            if (!Directory.Exists(destinationPath))
            {
                MessageBox.Show("The game needs to be installed through the launcher !");
                return;
            }

            // Create backup directory if it doesn't exist
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            if (Directory.GetFiles(backupPath).Length > 0)
            {
                MessageBox.Show("Shaders are already installed!");
                return;
            }

            // Move 4 files to backup directory
            string[] filesToMove = new string[]
            {
        "LegacyCubemap.material.bin",
        "RenderChunk.material.bin",
        "Sky.material.bin",
        "SunMoon.material.bin"
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
        "Sky.material.bin",
        "SunMoon.material.bin"
            };
            foreach (string file in filesToDownload)
            {
                string url = $"https://github.com/ignYoqzii/StarZLauncher/releases/download/shadersinstaller/{file}";
                string destinationFilePath = Path.Combine(destinationPath, file);
                using WebClient client = new();
                client.DownloadFile(url, destinationFilePath);
            }

            MessageBox.Show("Shaders installed successfully!");
        }

        public static void CosmeticsSkinPackApply()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(myDocumentsPath, "StarZ Launcher", "StarZ Versions", "StarZ X Minecraft", "data", "skin_packs");

            if (!Directory.Exists(destinationPath))
            {
                MessageBox.Show("The game needs to be installed through the launcher !");
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
                MessageBox.Show("Custom Persona installed successfully!");
            }
        }
    }
}
