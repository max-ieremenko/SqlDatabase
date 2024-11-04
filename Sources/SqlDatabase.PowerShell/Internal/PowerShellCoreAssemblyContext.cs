using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class PowerShellCoreAssemblyContext : AssemblyLoadContext
{
    private readonly AssemblyCache _privateCache;
    private readonly AssemblyCache _sharedCache;

    public PowerShellCoreAssemblyContext()
    {
        var common = GetType().Assembly.GetDirectoryLocation();

        _privateCache = new AssemblyCache(common, Path.Combine(common, "ps-core"));
        _sharedCache = new AssemblyCache(typeof(PSCmdlet).Assembly.GetDirectoryLocation());
    }

    public Assembly? LoadDependency(string assemblyName) => Load(new AssemblyName(assemblyName));

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var result = _privateCache.Load(assemblyName, LoadFromAssemblyPath);
        if (result == null)
        {
            result = _sharedCache.Load(assemblyName, Default.LoadFromAssemblyPath);
        }

        return result;
    }
}