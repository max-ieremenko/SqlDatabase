using System;
using System.Collections.Generic;

namespace SqlDatabase.Scripts.UpgradeInternal
{
    internal sealed class ModuleVersionResolver : IModuleVersionResolver
    {
        private readonly IDictionary<string, Version> _versionByModule = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);

        public ILogger Log { get; set; }

        public IDatabase Database { get; set; }

        public Version GetCurrentVersion(string moduleName)
        {
            if (moduleName == null)
            {
                moduleName = string.Empty;
            }

            if (!_versionByModule.TryGetValue(moduleName, out var version))
            {
                version = LoadVersion(moduleName);
                _versionByModule.Add(moduleName, version);
            }

            return version;
        }

        private Version LoadVersion(string moduleName)
        {
            if (moduleName.Length == 0)
            {
                Log.Info("get database version");
            }
            else
            {
                Log.Info("get module [{0}] version".FormatWith(moduleName));
            }

            var version = Database.GetCurrentVersion(moduleName);

            if (moduleName.Length == 0)
            {
                Log.Info("current database version is {0}".FormatWith(version));
            }
            else
            {
                Log.Info("current module [{0}] version is {1}".FormatWith(moduleName, version));
            }

            return version;
        }
    }
}
