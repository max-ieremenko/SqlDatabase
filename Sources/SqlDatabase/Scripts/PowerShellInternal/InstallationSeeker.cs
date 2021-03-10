using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SqlDatabase.Scripts.PowerShellInternal
{
    internal static class InstallationSeeker
    {
        public const string RootAssemblyName = "System.Management.Automation";
        private const string RootAssemblyFileName = RootAssemblyName + ".dll";

        public static bool TryFindByParentProcess(out string installationPath)
        {
            int processId;
            DateTime processStartTime;
            using (var current = Process.GetCurrentProcess())
            {
                processId = current.Id;
                processStartTime = current.StartTime;
            }

            installationPath = FindPowerShellProcess(processId, processStartTime);
            return !string.IsNullOrEmpty(installationPath) && IsPowerShellCore(installationPath);
        }

        public static bool TryFindOnDisk(out string installationPath)
        {
            installationPath = null;
            var root = GetDefaultInstallationRoot();
            if (!Directory.Exists(root))
            {
                return false;
            }

            var candidates = new List<InstallationInfo>();
            var directories = Directory.GetDirectories(root);
            for (var i = 0; i < directories.Length; i++)
            {
                var directory = directories[i];
                if (!IsPowerShellCore(directory))
                {
                    continue;
                }

                var info = GetVersion(directory);
                if (!string.IsNullOrEmpty(info.FileVersion)
                    && Version.TryParse(info.FileVersion, out var version)
                    && IsCompatibleVersion(version))
                {
                    candidates.Add(new InstallationInfo(directory, version, info.ProductVersion));
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            candidates.Sort();
            installationPath = candidates[candidates.Count - 1].Location;
            return true;
        }

        public static FileVersionInfo GetVersion(string installationPath)
        {
            return FileVersionInfo.GetVersionInfo(GetRootAssemblyFileName(installationPath));
        }

        public static bool IsPowerShellCore(string installationPath)
        {
            return File.Exists(GetRootAssemblyFileName(installationPath))
                && File.Exists(Path.Combine(installationPath, "pwsh.dll"));
        }

        public static string GetRootAssemblyFileName(string installationPath)
        {
            return Path.Combine(installationPath, RootAssemblyFileName);
        }

        private static string FindPowerShellProcess(int processId, DateTime processStartTime)
        {
            var parentId = DiagnosticsTools.GetParentProcessId(processId);
            if (!parentId.HasValue || parentId == processId)
            {
                return null;
            }

            string parentLocation = null;
            try
            {
                using (var parent = Process.GetProcessById(parentId.Value))
                {
                    if (parent.StartTime < processStartTime)
                    {
                        parentLocation = parent.MainModule?.FileName;
                    }
                }
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(parentLocation) || !File.Exists(parentLocation))
            {
                return null;
            }

            var fileName = Path.GetFileName(parentLocation);
            if (!"pwsh.exe".Equals(fileName, StringComparison.OrdinalIgnoreCase) && !"pwsh".Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                // try parent
                return FindPowerShellProcess(parentId.Value, processStartTime);
            }

            return Path.GetDirectoryName(fileName);
        }

        private static string GetDefaultInstallationRoot()
        {
            if (DiagnosticsTools.IsOSPlatformWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell");
            }

            return "/opt/microsoft/powershell";
        }

        private static bool IsCompatibleVersion(Version version)
        {
#if NET5_0
            return version < new Version("7.2");
#elif NETCOREAPP3_1_OR_GREATER
            return version < new Version("7.1");
#elif NETCOREAPP2_2_OR_GREATER
            return version < new Version("7.0");
#else
            return false;
#endif
        }

        [DebuggerDisplay("{Version}")]
        internal readonly struct InstallationInfo : IComparable<InstallationInfo>
        {
            public InstallationInfo(string location, Version version, string productVersion)
            {
                Location = location;
                Version = version;
                ProductVersion = productVersion ?? string.Empty;
            }

            public string Location { get; }

            public Version Version { get; }

            public string ProductVersion { get; }

            public int CompareTo(InstallationInfo other)
            {
                var result = Version.CompareTo(other.Version);
                if (result != 0)
                {
                    return result;
                }

                var isPreview = IsPreview();
                var otherIsPreview = other.IsPreview();
                if (isPreview && !otherIsPreview)
                {
                    return -1;
                }

                if (!isPreview && otherIsPreview)
                {
                    return 1;
                }

                result = StringComparer.InvariantCultureIgnoreCase.Compare(ProductVersion, other.ProductVersion);
                if (result == 0)
                {
                    result = StringComparer.InvariantCultureIgnoreCase.Compare(Location, other.Location);
                }

                return result;
            }

            private bool IsPreview()
            {
                return ProductVersion.IndexOf("preview", StringComparison.OrdinalIgnoreCase) > 0;
            }
        }
    }
}
