using System;

namespace SqlDatabase.Scripts
{
    public readonly struct ScriptDependency
    {
        public ScriptDependency(string moduleName, Version version)
        {
            ModuleName = moduleName;
            Version = version;
        }

        public string ModuleName { get; }

        public Version Version { get; }
    }
}