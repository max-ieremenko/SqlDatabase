using SqlDatabase.Adapter;

namespace SqlDatabase.Sequence;

internal sealed class ModuleVersionResolver : IModuleVersionResolver
{
    private readonly IDictionary<string, Version> _versionByModule = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);

    public ModuleVersionResolver(ILogger log, GetDatabaseCurrentVersion database)
    {
        Log = log;
        Database = database;
    }

    public ILogger Log { get; }

    public GetDatabaseCurrentVersion Database { get; internal set; }

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
        var version = Database(moduleName);

        if (moduleName.Length == 0)
        {
            Log.Info($"database version: {version}");
        }
        else
        {
            Log.Info($"module [{moduleName}] version: {version}");
        }

        return version;
    }
}