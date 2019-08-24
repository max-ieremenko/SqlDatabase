using System;

namespace SqlDatabase.Scripts
{
    public readonly struct ScriptDependency : IEquatable<ScriptDependency>
    {
        public ScriptDependency(string moduleName, Version version)
        {
            ModuleName = moduleName;
            Version = version;
        }

        public string ModuleName { get; }

        public Version Version { get; }

        public bool Equals(ScriptDependency other)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(ModuleName, other.ModuleName)
                   && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is ScriptDependency d && Equals(d);
        }

        public override int GetHashCode()
        {
            var h1 = StringComparer.OrdinalIgnoreCase.GetHashCode(ModuleName);
            var h2 = Version.GetHashCode();

            return (int)((uint)(h1 << 5) | (uint)h1 >> 27) + h1 ^ h2;
        }

        public override string ToString() => "{0} {1}".FormatWith(ModuleName, Version);
    }
}