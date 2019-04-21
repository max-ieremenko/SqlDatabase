using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Scripts
{
    public interface IDatabase
    {
        string ConnectionString { get; }

        string GetServerVersion();

        Version GetCurrentVersion();

        void Execute(IScript script, Version currentVersion, Version targetVersion);

        void Execute(IScript script);

        IEnumerable<IDataReader> ExecuteReader(IScript script);
    }
}
