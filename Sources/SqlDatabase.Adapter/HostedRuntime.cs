namespace SqlDatabase.Adapter;

public enum FrameworkVersion
{
    Net472,
    Net8,
    Net9,
    Net10
}

public readonly struct HostedRuntime
{
    public HostedRuntime(bool isPowershell, bool isWindows, FrameworkVersion version)
    {
        IsPowershell = isPowershell;
        IsWindows = isWindows;
        Version = version;
    }

    public bool IsPowershell { get; }

    public bool IsWindows { get; }

    public FrameworkVersion Version { get; }
}