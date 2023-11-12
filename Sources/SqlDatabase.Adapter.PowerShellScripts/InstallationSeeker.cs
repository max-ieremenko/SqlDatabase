using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class InstallationSeeker
{
    public const string RootAssemblyName = "System.Management.Automation";
    public const string RootAssemblyFileName = RootAssemblyName + ".dll";

    public static bool TryFindByParentProcess([NotNullWhen(true)] out string? installationPath)
    {
        int processId;
        DateTime processStartTime;
        using (var current = Process.GetCurrentProcess())
        {
            processId = current.Id;
            processStartTime = current.StartTime;
        }

        installationPath = FindPowerShellProcess(processId, processStartTime);
        return !string.IsNullOrEmpty(installationPath)
               && TryGetInfo(installationPath!, out var info)
               && IsCompatibleVersion(info.Version);
    }

    public static bool TryFindOnDisk([NotNullWhen(true)] out string? installationPath)
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
            if (TryGetInfo(directories[i], out var info)
                && IsCompatibleVersion(info.Version))
            {
                candidates.Add(info);
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

    public static bool TryGetInfo(string installationPath, out InstallationInfo info)
    {
        info = default;

        var root = Path.Combine(installationPath, RootAssemblyFileName);
        if (!File.Exists(Path.Combine(installationPath, "pwsh.dll"))
            || !File.Exists(root))
        {
            return false;
        }

        var fileInfo = FileVersionInfo.GetVersionInfo(root);
        if (string.IsNullOrEmpty(fileInfo.FileVersion)
            || string.IsNullOrEmpty(fileInfo.ProductVersion)
            || !Version.TryParse(fileInfo.FileVersion, out var version))
        {
            return false;
        }

        info = new InstallationInfo(installationPath, version, fileInfo.ProductVersion);
        return true;
    }

    private static string? FindPowerShellProcess(int processId, DateTime processStartTime)
    {
        var parentId = DiagnosticsTools.GetParentProcessId(processId);
        if (!parentId.HasValue || parentId == processId)
        {
            return null;
        }

        string? parentLocation = null;
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

        return Path.GetDirectoryName(parentLocation);
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
#if NET8_0
        return version < new Version("7.5");
#elif NET7_0
        return version < new Version("7.4");
#elif NET6_0
        return version < new Version("7.3");
#elif NET5_0
        return version < new Version("7.2");
#elif NETCOREAPP3_1_OR_GREATER
        return version < new Version("7.1");
#else
        return false;
#endif
    }
}