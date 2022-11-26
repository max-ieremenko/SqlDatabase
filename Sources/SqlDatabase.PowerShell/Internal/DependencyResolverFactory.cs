using System;
using System.Management.Automation;

namespace SqlDatabase.PowerShell.Internal;

internal static class DependencyResolverFactory
{
    public static IDependencyResolver Create(PSCmdlet cmdlet)
    {
        if (!cmdlet.TryGetPSVersionTable(out var psVersionTable))
        {
            throw new PlatformNotSupportedException("$PSVersionTable is not defined.");
        }

        return Create(psVersionTable);
    }

    internal static IDependencyResolver Create(PSVersionTable psVersionTable)
    {
        // In PowerShell 4 and below, this variable does not exist
        if (string.IsNullOrEmpty(psVersionTable.PSEdition) || "Desktop".Equals(psVersionTable.PSEdition, StringComparison.OrdinalIgnoreCase))
        {
            return new PowerShellDesktopDependencyResolver();
        }

        return new PowerShellCoreDependencyResolver();
    }
}