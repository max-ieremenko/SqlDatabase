using System.Reflection;
using System.Runtime.Loader;

namespace SqlDatabase.Adapter.AssemblyScripts.NetCore;

internal sealed class AssemblyContext : AssemblyLoadContext
{
    public Assembly? ScriptAssembly { get; private set; }

    public void LoadScriptAssembly(byte[] assemblyContent)
    {
        using (var stream = new MemoryStream(assemblyContent))
        {
            ScriptAssembly = LoadFromStream(stream);
        }
    }

    public void UnloadAll()
    {
        ScriptAssembly = null;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var isScriptAssembly = assemblyName.Name != null
                               && assemblyName.Name.Equals(ScriptAssembly?.GetName().Name, StringComparison.OrdinalIgnoreCase);
        return isScriptAssembly ? ScriptAssembly : null;
    }
}