using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class PowerShellDesktopDependencyResolver : IDependencyResolver
{
    private readonly AssemblyCache _cache;

    public PowerShellDesktopDependencyResolver()
    {
        var psDesktop = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ps-desktop");
        _cache = new AssemblyCache(psDesktop);
    }

    public void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        _cache.Dispose();
    }

    private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
    {
        return _cache.Load(new AssemblyName(args.Name), Assembly.LoadFrom);
    }
}