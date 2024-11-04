using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal interface IDependencyResolver
{
    void Attach();

    void Detach();

    Assembly? LoadDependency(string assemblyName);
}