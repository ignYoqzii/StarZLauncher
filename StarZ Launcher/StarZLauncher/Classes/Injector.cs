using StarZLauncher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace StarZLauncher.Classes
{
    public static class Injector
    {
        private static readonly SecurityIdentifier AppPackagesSid = new("S-1-15-2-1");

        public static async Task<bool> Inject(string path)
        {
            string logFileName = $"{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Logs", logFileName);

            try
            {
                Log($"Injection process started at: {DateTime.Now}", logFilePath);
                ApplyAppPackages(path, logFilePath);

                Process process = GetMinecraftProcess();
                if (process == null)
                {
                    Log("Failed to find Minecraft process.", logFilePath);
                    return false;
                }

                Log($"Found Minecraft process with PID: {process.Id}", logFilePath);

                IntPtr processHandle = IntPtr.Zero;
                IntPtr threadHandle = IntPtr.Zero;

                try
                {
                    processHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_ALL_ACCESS, false, process.Id);
                    if (processHandle == IntPtr.Zero)
                    {
                        Log("Failed to open process handle.", logFilePath);
                        return false;
                    }

                    Log("Process handle opened successfully.", logFilePath);

                    IntPtr loadLibraryAddress = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    if (loadLibraryAddress == IntPtr.Zero)
                    {
                        Log("Failed to get address of LoadLibraryA.", logFilePath);
                        return false;
                    }

                    Log("Address of LoadLibraryA obtained successfully.", logFilePath);

                    uint size = (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char)));
                    IntPtr allocMemAddress = NativeMethods.VirtualAllocEx(processHandle, IntPtr.Zero, size, NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);
                    if (allocMemAddress == IntPtr.Zero)
                    {
                        Log("Failed to allocate memory in remote process.", logFilePath);
                        return false;
                    }

                    Log("Memory allocated in remote process successfully.", logFilePath);

                    byte[] buffer = Encoding.Default.GetBytes(path); // Use ANSI encoding for LoadLibraryA
                    if (!NativeMethods.WriteProcessMemory(processHandle, allocMemAddress, buffer, (uint)buffer.Length, out IntPtr bytesWritten) || bytesWritten.ToInt32() != buffer.Length)
                    {
                        Log($"Failed to write to allocated memory in remote process or partial write detected. Bytes written: {bytesWritten.ToInt32()}, Buffer length: {buffer.Length}", logFilePath);
                        return false;
                    }

                    Log($"Library path written to allocated memory in remote process. Bytes written: {bytesWritten.ToInt32()}, Buffer length: {buffer.Length}", logFilePath);

                    threadHandle = NativeMethods.CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddress, allocMemAddress, 0, IntPtr.Zero);
                    if (threadHandle == IntPtr.Zero)
                    {
                        Log("Failed to create remote thread.", logFilePath);
                        return false;
                    }

                    Log("Remote thread created successfully.", logFilePath);

                    // Wait for the module to load asynchronously
                    await WaitForModuleLoad(process, path, logFilePath);

                    // Wait for the remote thread to complete
                    uint waitResult = NativeMethods.WaitForSingleObject(threadHandle, 10000);
                    if (waitResult != 0)
                    {
                        Log($"Remote thread did not complete in a timely manner. Wait result: {waitResult}", logFilePath);
                        return false;
                    }

                    Log("Remote thread completed successfully.", logFilePath);
                }
                finally
                {
                    if (processHandle != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(processHandle);
                        Log("Process handle closed.", logFilePath);
                    }
                    if (threadHandle != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(threadHandle);
                        Log("Thread handle closed.", logFilePath);
                    }
                }

                Log("Injection process completed successfully.", logFilePath);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error injecting library: {ex.Message}", logFilePath);
                return false;
            }
        }

        private static Process GetMinecraftProcess()
        {
            var processes = Process.GetProcessesByName("Minecraft.Windows");
            return processes.FirstOrDefault(p => !p.HasExited && p.Responding);
        }

        private static void ApplyAppPackages(string path, string logFilePath)
        {
            var fileInfo = new FileInfo(path);
            try
            {
                var fileSecurity = fileInfo.GetAccessControl();
                fileSecurity.AddAccessRule(new FileSystemAccessRule(AppPackagesSid, FileSystemRights.FullControl, AccessControlType.Allow));
                fileInfo.SetAccessControl(fileSecurity);
                Log("Applied AppPackages permissions to the DLL file.", logFilePath);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply AppPackages permissions: {ex.Message}", logFilePath);
            }
        }

        private static async Task WaitForModuleLoad(Process process, string path, string logFilePath)
        {
            int attempt = 0;
            while (!IsInjected(process, path))
            {
                Log($"Checking if module is loaded, attempt {++attempt}", logFilePath);
                await Task.Delay(100);
                process.Refresh();
            }

            Log("Injected module detected in Minecraft process.", logFilePath);
        }

        public static bool IsInjected(Process process, string path)
        {
            if (process == null || process.HasExited || !process.Responding)
                return false;

            return process.Modules.Cast<ProcessModule>().Any(m => m.FileName.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private static void Log(string message, string logFilePath)
        {
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Failed to write to log file: {ex.Message}", "Error", false);
            }
        }

        private static void LogError(string message, string logFilePath)
        {
            Log($"ERROR: {message}", logFilePath);
        }
    }

    internal static class NativeMethods
    {
        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_READWRITE = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    }
}