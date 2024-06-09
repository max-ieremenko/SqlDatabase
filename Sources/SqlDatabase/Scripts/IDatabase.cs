using SqlDatabase.Adapter;

namespace SqlDatabase.Scripts;

internal interface IDatabase
{
    IDatabaseAdapter Adapter { get; }

    string GetServerVersion(bool useMasterDatabase);

    Version GetCurrentVersion(string? moduleName);

    void Execute(IScript script, string moduleName, Version currentVersion, Version targetVersion);

    void Execute(IScript script);

    void ExecuteWithDatabaseCheck(IScript script);

    IEnumerable<IDataReader> ExecuteReader(IScript script);
}