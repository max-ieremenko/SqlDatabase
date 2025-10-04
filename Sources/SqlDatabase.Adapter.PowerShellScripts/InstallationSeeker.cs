namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class InstallationSeeker
{
    public const string PowershellFileName = "pwsh.dll";
    public const string RootAssemblyName = "System.Management.Automation";
    public const string RootAssemblyFileName = RootAssemblyName + ".dll";

    public static bool TryFindByParentProcess(HostedRuntime runtime, [NotNullWhen(true)] out string? installationPath)
    {
        installationPath = DiagnosticsTools.FindPowerShellProcess(runtime);

        return !string.IsNullOrEmpty(installationPath)
               && TryGetInfo(installationPath!, out var info)
               && IsCompatibleVersion(runtime.Version, info.Version);
    }

    public static bool TryFindOnDisk(HostedRuntime runtime, [NotNullWhen(true)] out string? installationPath)
    {
        installationPath = null;

        var root = runtime.IsWindows ? PowerShellWindows.GetInstallationPath() : PowerShellLinux.GetInstallationPath();
        if (!Directory.Exists(root))
        {
            return false;
        }

        var candidates = new List<InstallationInfo>();

        var directories = Directory.GetDirectories(root);
        for (var i = 0; i < directories.Length; i++)
        {
            if (TryGetInfo(directories[i], out var info)
                && IsCompatibleVersion(runtime.Version, info.Version))
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
        if (!File.Exists(Path.Combine(installationPath, PowershellFileName))
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

    private static bool IsCompatibleVersion(FrameworkVersion runtimeVersion, Version version)
    {
        if (runtimeVersion == FrameworkVersion.Net9)
        {
            return version <= new Version("7.6");
        }

        if (runtimeVersion == FrameworkVersion.Net8)
        {
            return version < new Version("7.5");
        }

        return false;
    }
}