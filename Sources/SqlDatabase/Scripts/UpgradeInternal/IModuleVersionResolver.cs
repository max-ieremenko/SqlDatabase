using System;

namespace SqlDatabase.Scripts.UpgradeInternal
{
    internal interface IModuleVersionResolver
    {
        // => InvalidOperationException fail to determine dependent module [{1}] version
        Version GetCurrentVersion(string moduleName);
    }
}
