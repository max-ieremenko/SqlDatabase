using System;

namespace SqlDatabase.Scripts
{
    public interface IUpgradeDatabase
    {
        Version GetCurrentVersion();

        void BeforeUpgrade();

        void Execute(IScript script, Version currentVersion, Version targetVersion);
    }
}