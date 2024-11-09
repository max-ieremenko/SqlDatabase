using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class AssemblyCache
{
    private readonly string[] _probingPaths;

    public AssemblyCache(params string[] probingPaths)
    {
        _probingPaths = probingPaths;
    }

    public Assembly? Load(AssemblyName assemblyName, Func<string, Assembly> loader)
    {
        var fileName = assemblyName.Name + ".dll";
        return TryFindAndLoad(fileName, loader);
    }

    private Assembly? TryFindAndLoad(string fileName, Func<string, Assembly> loader)
    {
        for (var i = 0; i < _probingPaths.Length; i++)
        {
            var path = Path.Combine(_probingPaths[i], fileName);
            if (File.Exists(path))
            {
                return loader(path);
            }
        }

        return null;
    }
}