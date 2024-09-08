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
        private static readonly string logFileName = "Injector.txt";

        public static async Task<bool> Inject(string path)
        {

            try
            {
                LogManager.Log($"Injection process started at: {DateTime.Now}", logFileName);
                ApplyAppPackages(path);

                Process process = GetMinecraftProcess();
                if (process == null)
                {
                    LogManager.Log("Failed to find Minecraft process.", logFileName);
                    return false;
                }

                LogManager.Log($"Found Minecraft process with PID: {process.Id}", logFileName);

                IntPtr processHandle = IntPtr.Zero;
                IntPtr threadHandle = IntPtr.Zero;

                try
                {
                    processHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_ALL_ACCESS, false, process.Id);
                    if (processHandle == IntPtr.Zero)
                    {
                        LogManager.Log("Failed to open process handle.", logFileName);
                        return false;
                    }

                    LogManager.Log("Process handle opened successfully.", logFileName);

                    IntPtr loadLibraryAddress = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    if (loadLibraryAddress == IntPtr.Zero)
                    {
                        LogManager.Log("Failed to get address of LoadLibraryA.", logFileName);
                        return false;
                    }

                    LogManager.Log("Address of LoadLibraryA obtained successfully.", logFileName);

                    uint size = (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char)));
                    IntPtr allocMemAddress = NativeMethods.VirtualAllocEx(processHandle, IntPtr.Zero, size, NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);
                    if (allocMemAddress == IntPtr.Zero)
                    {
                        LogManager.Log("Failed to allocate memory in remote process.", logFileName);
                        return false;
                    }

                    LogManager.Log("Memory allocated in remote process successfully.", logFileName);

                    byte[] buffer = Encoding.Default.GetBytes(path); // Use ANSI encoding for LoadLibraryA
                    if (!NativeMethods.WriteProcessMemory(processHandle, allocMemAddress, buffer, (uint)buffer.Length, out IntPtr bytesWritten) || bytesWritten.ToInt32() != buffer.Length)
                    {
                        LogManager.Log($"Failed to write to allocated memory in remote process or partial write detected. Bytes written: {bytesWritten.ToInt32()}, Buffer length: {buffer.Length}", logFileName);
                        return false;
                    }

                    LogManager.Log($"Library path written to allocated memory in remote process. Bytes written: {bytesWritten.ToInt32()}, Buffer length: {buffer.Length}", logFileName);

                    threadHandle = NativeMethods.CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddress, allocMemAddress, 0, IntPtr.Zero);
                    if (threadHandle == IntPtr.Zero)
                    {
                        LogManager.Log("Failed to create remote thread.", logFileName);
                        return false;
                    }

                    LogManager.Log("Remote thread created successfully.", logFileName);

                    // Wait for the module to load asynchronously
                    await WaitForModuleLoad(process, path);

                    // Wait for the remote thread to complete
                    uint waitResult = NativeMethods.WaitForSingleObject(threadHandle, 10000);
                    if (waitResult != 0)
                    {
                        LogManager.Log($"Remote thread did not complete in a timely manner. Wait result: {waitResult}", logFileName);
                        return false;
                    }

                    LogManager.Log("Remote thread completed successfully.", logFileName);
                }
                finally
                {
                    if (processHandle != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(processHandle);
                        LogManager.Log("Process handle closed.", logFileName);
                    }
                    if (threadHandle != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(threadHandle);
                        LogManager.Log("Thread handle closed.", logFileName);
                    }
                }

                LogManager.Log("Injection process completed successfully.", logFileName);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log($"ERROR: {ex.Message}", logFileName);
                return false;
            }
        }

        private static Process GetMinecraftProcess()
        {
            var processes = Process.GetProcessesByName("Minecraft.Windows");
            return processes.FirstOrDefault(p => !p.HasExited && p.Responding);
        }

        private static void ApplyAppPackages(string path)
        {
            var fileInfo = new FileInfo(path);
            try
            {
                var fileSecurity = fileInfo.GetAccessControl();
                fileSecurity.AddAccessRule(new FileSystemAccessRule(AppPackagesSid, FileSystemRights.FullControl, AccessControlType.Allow));
                fileInfo.SetAccessControl(fileSecurity);
                LogManager.Log("Applied AppPackages permissions to the DLL file.", logFileName);
            }
            catch (Exception ex)
            {
                LogManager.Log($"ERROR: Failed to apply AppPackages permissions: {ex.Message}", logFileName);
            }
        }

        private static async Task WaitForModuleLoad(Process process, string path)
        {
            int attempt = 0;
            while (!IsInjected(process, path))
            {
                LogManager.Log($"Checking if module is loaded, attempt {++attempt}", logFileName);
                await Task.Delay(100);
                process.Refresh();
            }

            LogManager.Log("Injected module detected in Minecraft process.", logFileName);
        }

        public static bool IsInjected(Process process, string path)
        {
            if (process == null || process.HasExited || !process.Responding)
                return false;

            return process.Modules.Cast<ProcessModule>().Any(m => m.FileName.Equals(path, StringComparison.OrdinalIgnoreCase));
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
