using System;

namespace SqlDatabase.Scripts
{
    public interface IDatabase
    {
        Version GetCurrentVersion();

        void BeforeUpgrade();

        void ExecuteUpgrade(IScript script, Version currentVersion, Version targetVersion);
    }
}