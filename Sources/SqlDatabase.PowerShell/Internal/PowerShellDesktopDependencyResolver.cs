using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class PowerShellDesktopDependencyResolver : IDependencyResolver
{
    private readonly AssemblyCache _cache;
    private int _refCounter;

    public PowerShellDesktopDependencyResolver()
    {
        var common = GetType().Assembly.GetDirectoryLocation();
        _cache = new AssemblyCache(common, Path.Combine(common, "ps-desktop"));
    }

    public void Attach()
    {
        if (Interlocked.Increment(ref _refCounter) == 1)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }
    }

    public void Detach()
    {
        if (Interlocked.Decrement(ref _refCounter) == 0)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        }
    }

    public Assembly? LoadDependency(string assemblyName) => _cache.Load(new AssemblyName(assemblyName), Assembly.LoadFrom);

    private Assembly? AssemblyResolve(object sender, ResolveEventArgs args) => LoadDependency(args.Name);
}