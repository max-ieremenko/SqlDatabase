using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Scripts;

internal interface IDatabase
{
    IDatabaseAdapter Adapter { get; }

    string GetServerVersion();

    Version GetCurrentVersion(string moduleName);

    void Execute(IScript script, string moduleName, Version currentVersion, Version targetVersion);

    void Execute(IScript script);

    IEnumerable<IDataReader> ExecuteReader(IScript script);
}