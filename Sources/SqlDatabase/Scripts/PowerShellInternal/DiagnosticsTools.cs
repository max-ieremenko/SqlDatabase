using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SqlDatabase.Scripts.PowerShellInternal
{
    internal static class DiagnosticsTools
    {
        public static bool IsOSPlatformWindows()
        {
#if NET452
            return true;
#else
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
        }

        public static int? GetParentProcessId(int processId)
        {
#if NET452
            return null;
#else
            return IsOSPlatformWindows() ? GetParentProcessIdWindows(processId) : GetParentProcessIdLinux(processId);
#endif
        }

        internal static int? ParseParentProcessIdLinux(string fileName)
        {
            string line = null;

            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true))
                {
                    line = reader.ReadLine();
                }
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            // (2) comm  %s: The filename of the executable, in parentheses.
            var startIndex = line.LastIndexOf(')');
            if (startIndex <= 0 || startIndex >= line.Length)
            {
                return null;
            }

            // (3) state  %c
            startIndex = line.IndexOf(' ', startIndex + 1);
            if (startIndex <= 0 || startIndex >= line.Length)
            {
                return null;
            }

            // (4) ppid  %d: The PID of the parent of this process.
            startIndex = line.IndexOf(' ', startIndex + 1);
            if (startIndex <= 0 || startIndex >= line.Length)
            {
                return null;
            }

            var endIndex = line.IndexOf(' ', startIndex + 1);
            if (endIndex <= startIndex)
            {
                return null;
            }

            var ppid = line.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (int.TryParse(ppid, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

#if !NET452
        private static int? GetParentProcessIdLinux(int processId)
        {
            // /proc/[pid]/stat https://man7.org/linux/man-pages/man5/procfs.5.html
            var fileName = "/proc/" + processId.ToString(CultureInfo.InvariantCulture) + "/stat";

            return ParseParentProcessIdLinux(fileName);
        }

        private static int? GetParentProcessIdWindows(int processId)
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
#endif
    }
}
