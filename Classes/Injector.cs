using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static StarZLauncher.MainWindow;

namespace StarZLauncher.Classes
{
    // class to inject the dlls
    public static class Injector
    {
        public static void Inject(string path)
        {

            // A lot of this is from https://github.com/notcarlton (carlton#0002) and Plextora#0033 / Modified by Yoqzii

            try
            {
                ApplyAppPackages(path);

                var targetProcess = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();

                if (targetProcess != null)
                {
                    var procHandle = Resources.OpenProcess(Resources.PROCESS_CREATE_THREAD | Resources.PROCESS_QUERY_INFORMATION | Resources.PROCESS_VM_OPERATION | Resources.PROCESS_VM_WRITE | Resources.PROCESS_VM_READ, false, targetProcess.Id);

                    var loadLibraryAddress = Resources.GetProcAddress(Resources.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                    var allocMemAddress = Resources.VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), Resources.MEM_COMMIT | Resources.MEM_RESERVE, Resources.PAGE_READWRITE);

                    Resources.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path), (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
                    Resources.CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddress, allocMemAddress, 0, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error injecting library: {ex}");
            }
        }

        private static void ApplyAppPackages(string path)
        {
            var infoFile = new FileInfo(path);
            var fSecurity = infoFile.GetAccessControl();
            fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

            infoFile.SetAccessControl(fSecurity);
        }

        public static async Task WaitForModules()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                });
                while (true)
                {
                    try
                    {
                        Minecraft?.Refresh();
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show($"Minecraft was exited before injecting: {ex.Message}", "StarZ crashed =(");
                    }

                    if (Minecraft?.Modules.Count > 160) break;
                    Thread.Sleep(4000);
                }
            });
        }

    }
}

public class Resources
{
    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION = 0x2;

    public const int PROCESS_CREATE_THREAD = 0x0002;
    public const int PROCESS_QUERY_INFORMATION = 0x0400;
    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_VM_WRITE = 0x0020;
    public const int PROCESS_VM_READ = 0x0010;

    public const uint MEM_COMMIT = 0x00001000;
    public const uint MEM_RESERVE = 0x00002000;
    public const uint PAGE_READWRITE = 4;

    [DllImport("user32.dll")]
    public static extern int SendMessage(nint hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("kernel32.dll")]
    public static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern nint GetProcAddress(nint hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    public static extern nint VirtualAllocEx(nint hProcess, nint lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern nint CreateRemoteThread(nint hProcess, nint lpThreadAttributes, uint dwStackSize, nint lpStartAddress, nint lpParameter, uint dwCreationFlags, nint lpThreadId);
}
