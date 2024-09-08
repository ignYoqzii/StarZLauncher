using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace StarZLauncher.Classes
{
    public class DeveloperModeManager
    {
        private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock";
        private const string AllowAllTrustedAppsValueName = "AllowAllTrustedApps";
        private const string AllowDevelopmentWithoutDevLicenseValueName = "AllowDevelopmentWithoutDevLicense";

        public static void EnableDeveloperMode()
        {
            if (IsDeveloperModeEnabled())
            {
                return;
            }

            string[] commands =
            {
                @$"reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"" /t REG_DWORD /f /v ""{AllowAllTrustedAppsValueName}"" /d ""1""",
                @$"reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"" /t REG_DWORD /f /v ""{AllowDevelopmentWithoutDevLicenseValueName}"" /d ""1"""
            };

            foreach (var command in commands)
            {
                ExecutePowerShellCommand(command);
            }
        }

        private static void ExecutePowerShellCommand(string command)
        {
            ProcessStartInfo psi = new("powershell.exe", command)
            {
                Verb = "runas", // Run PowerShell as administrator
                CreateNoWindow = true, // Run without opening a window
                WindowStyle = ProcessWindowStyle.Hidden // Hide the window
            };

            try
            {
                using (Process process = Process.Start(psi))
                {
                    process?.WaitForExit();

                    // Check if the process exited with a non-zero exit code, indicating an error
                    if (process != null && process.ExitCode != 0)
                    {
                        throw new Exception($"PowerShell command failed with exit code {process.ExitCode}.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error enabling Developer Mode. It is recommended to enable it manually. {ex.Message}");
            }
        }

        public static bool IsDeveloperModeEnabled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, false);
                if (key == null)
                {
                    throw new Exception("Registry key not found.");
                }

                var value = key.GetValue(AllowDevelopmentWithoutDevLicenseValueName);
                return value is int intValue && intValue == 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}