#if !NET472
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SqlDatabase.Scripts.AssemblyInternal.NetCore
{
    internal sealed class AssemblyContext : AssemblyLoadContext
    {
        public Assembly ScriptAssembly { get; private set; }

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

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var isScriptAssembly = assemblyName.Name.Equals(ScriptAssembly.GetName().Name, StringComparison.OrdinalIgnoreCase);
            return isScriptAssembly ? ScriptAssembly : null;
        }
    }
}
#endif