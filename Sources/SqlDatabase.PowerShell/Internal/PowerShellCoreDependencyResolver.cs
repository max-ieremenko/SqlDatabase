using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class PowerShellCoreDependencyResolver : IDependencyResolver
{
    private readonly PowerShellCoreAssemblyContext _context = new();

    public void Attach()
    {
    }

    public void Detach()
    {
    }

    public Assembly? LoadDependency(string assemblyName) => _context.LoadDependency(assemblyName);
}