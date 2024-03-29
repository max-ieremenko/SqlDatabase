using System.Runtime.InteropServices;
using SqlDatabase.Adapter;

namespace SqlDatabase.Configuration;

internal static class HostedRuntimeResolver
{
    public static HostedRuntime GetRuntime(bool isPowershell)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var version = ResolveVersion(RuntimeInformation.FrameworkDescription, Environment.Version);

        if (version == FrameworkVersion.Net472 && !isWindows)
        {
            Throw();
        }

        return new HostedRuntime(isPowershell, isWindows, version);
    }

    public static bool SupportUsePowerShell(this HostedRuntime runtime) => !runtime.IsPowershell && runtime.Version != FrameworkVersion.Net472;

    // https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
    internal static FrameworkVersion ResolveVersion(string description, Version version)
    {
        if (description.IndexOf(" Framework ", StringComparison.OrdinalIgnoreCase) > 0)
        {
            return FrameworkVersion.Net472;
        }

        if (description.IndexOf(" Core ", StringComparison.OrdinalIgnoreCase) > 0)
        {
            return FrameworkVersion.Net6;
        }

        switch (version.Major)
        {
            case <= 6:
                return FrameworkVersion.Net6;
            case 7:
                return FrameworkVersion.Net7;
            default:
                return FrameworkVersion.Net8;
        }
    }

    private static void Throw()
    {
        var message = new StringBuilder("Runtime framework version is not supported. OSDescription: ")
            .Append(RuntimeInformation.OSDescription)
            .Append(", EnvironmentVersion: ")
            .Append(Environment.Version)
            .Append(", FrameworkDescription: ")
            .Append(RuntimeInformation.FrameworkDescription);

        throw new NotSupportedException(message.ToString());
    }
}