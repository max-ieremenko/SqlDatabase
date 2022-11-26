using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class PowerShellCoreDependencyResolver : IDependencyResolver
{
    private readonly AssemblyCache _cache;

    public PowerShellCoreDependencyResolver()
    {
        var psCore = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ps-core");
        _cache = new AssemblyCache(
            psCore,
            Path.GetDirectoryName(typeof(PSCmdlet).Assembly.Location));
    }

    public void Initialize()
    {
        // Fail to load configuration from [SqlDatabase.exe.config].
        // ---> An error occurred creating the configuration section handler for sqlDatabase: Could not load file or assembly 'SqlDatabase, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified.
        AssemblyLoadContext.Default.Resolving += AssemblyResolving;
    }

    public void Dispose()
    {
        AssemblyLoadContext.Default.Resolving -= AssemblyResolving;
        _cache.Dispose();
    }

    private Assembly AssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return _cache.Load(assemblyName, context.LoadFromAssemblyPath);
    }
}