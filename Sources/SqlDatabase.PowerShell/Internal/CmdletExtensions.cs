using System.Management.Automation;

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
}