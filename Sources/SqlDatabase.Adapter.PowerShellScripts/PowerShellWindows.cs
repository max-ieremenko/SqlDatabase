using System.Runtime.InteropServices;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class PowerShellWindows
{
    public static string GetInstallationPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell");

    public static bool IsExecutable(string fileName) => "pwsh.exe".Equals(fileName, StringComparison.OrdinalIgnoreCase);

    public static int? GetParentProcessId(int processId)
    {
        const int DesiredAccess = 0x0400; // PROCESS_QUERY_INFORMATION
        const int ProcessInfoClass = 0; // ProcessBasicInformation

        int? result = null;
        using (var hProcess = OpenProcess(DesiredAccess, false, processId))
        {
            if (!hProcess.IsInvalid)
            {
                var basicInformation = default(ProcessBasicInformation);
                var pSize = 0;
                var pbiSize = (uint)Marshal.SizeOf<ProcessBasicInformation>();

                if (NtQueryInformationProcess(hProcess, ProcessInfoClass, ref basicInformation, pbiSize, ref pSize) == 0)
                {
                    result = (int)basicInformation.InheritedFromUniqueProcessId;
                }
            }
        }

        return result;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle hProcess, int pic, ref ProcessBasicInformation pbi, uint cb, ref int pSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessBasicInformation
    {
        public uint ExitStatus;
        public IntPtr PebBaseAddress;
        public UIntPtr AffinityMask;
        public int BasePriority;
        public UIntPtr UniqueProcessId;
        public UIntPtr InheritedFromUniqueProcessId;
    }
}