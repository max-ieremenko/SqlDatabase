using System.Management.Automation;
using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal static class CmdletExtensions
{
    public static string GetWorkingDirectory(this PSCmdlet cmdlet)
    {
        var root = cmdlet.MyInvocation.PSScriptRoot;
        if (string.IsNullOrEmpty(root))
        {
            root = cmdlet.CurrentProviderLocation("FileSystem").ProviderPath;
        }

        return root;
    }

    public static bool TryGetPSVersionTable(this PSCmdlet cmdlet, out PSVersionTable value)
    {
        var psVersionTable = cmdlet.GetVariableValue("PSVersionTable");
        if (psVersionTable == null)
        {
            value = default;
            return false;
        }

        value = new PSVersionTable(psVersionTable);
        return true;
    }

    public static string GetDirectoryLocation(this Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location) || !File.Exists(location))
        {
            throw new InvalidOperationException($"Location of {assembly.FullName} not found '{location}'.");
        }

        return Path.GetDirectoryName(location)!;
    }
}