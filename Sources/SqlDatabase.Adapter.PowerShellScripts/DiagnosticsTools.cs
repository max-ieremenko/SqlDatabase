namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class DiagnosticsTools
{
    public static string? FindPowerShellProcess(HostedRuntime runtime)
    {
        if (runtime.Version == FrameworkVersion.Net472)
        {
            return null;
        }

        int processId;
        DateTime processStartTime;
        using (var current = Process.GetCurrentProcess())
        {
            processId = current.Id;
            processStartTime = current.StartTime;
        }

        return TryParentProcess(runtime, processId, processStartTime);
    }

    private static string? TryParentProcess(HostedRuntime runtime, int childId, DateTime processStartTime)
    {
        var parentId = runtime.IsWindows ? PowerShellWindows.GetParentProcessId(childId) : PowerShellLinux.GetParentProcessId(childId);
        if (!parentId.HasValue || parentId == childId)
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
        if (!PowerShellWindows.IsExecutable(fileName) && PowerShellLinux.IsExecutable(fileName))
        {
            // try parent
            return TryParentProcess(runtime, parentId.Value, processStartTime);
        }

        return Path.GetDirectoryName(parentLocation);
    }
}