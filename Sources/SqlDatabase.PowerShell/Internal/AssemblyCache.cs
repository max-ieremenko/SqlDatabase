using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class AssemblyCache : IDisposable
{
    private readonly string[] _probingPaths;
    private readonly IDictionary<string, Assembly?> _assemblyByName;

    public AssemblyCache(params string[] probingPaths)
    {
        _probingPaths = new string[probingPaths.Length + 1];
        _probingPaths[0] = Path.GetDirectoryName(GetType().Assembly.Location);
        for (var i = 0; i < probingPaths.Length; i++)
        {
            _probingPaths[i + 1] = probingPaths[i];
        }

        _assemblyByName = new Dictionary<string, Assembly?>(StringComparer.OrdinalIgnoreCase);
    }

    public Assembly? Load(AssemblyName assemblyName, Func<string, Assembly> loader)
    {
        var fileName = assemblyName.Name + ".dll";
        if (_assemblyByName.TryGetValue(fileName, out var assembly))
        {
            return assembly;
        }

        assembly = TryFindAndLoad(fileName, loader);
        _assemblyByName[fileName] = assembly;
        return assembly;
    }

    public void Dispose()
    {
        _assemblyByName.Clear();
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