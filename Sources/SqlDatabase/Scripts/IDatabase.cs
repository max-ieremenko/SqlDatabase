using System;

namespace SqlDatabase.Scripts
{
    public interface IDatabase
    {
        string ConnectionString { get; }

        string GetServerVersion();

        Version GetCurrentVersion();

        void Execute(IScript script, Version currentVersion, Version targetVersion);

        void Execute(IScript script);
    }
}
