using System;
using System.Collections.Generic;
using SqlDatabase.Adapter;

namespace SqlDatabase.Scripts.UpgradeInternal;

internal sealed class ModuleVersionResolver : IModuleVersionResolver
{
    private readonly IDictionary<string, Version> _versionByModule = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);

    public ModuleVersionResolver(ILogger log, IDatabase database)
    {
        Log = log;
        Database = database;
    }

    public ILogger Log { get; }

    public IDatabase Database { get; }

    public Version GetCurrentVersion(string? moduleName)
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
        var version = Database.GetCurrentVersion(moduleName);

        if (moduleName.Length == 0)
        {
            Log.Info("database version: {0}".FormatWith(version));
        }
        else
        {
            Log.Info("module [{0}] version: {1}".FormatWith(moduleName, version));
        }

        return version;
    }
}