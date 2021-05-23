using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    // do not load SqlDatabase.dll on Import-Module
    public enum PSTransactionMode
    {
        None = TransactionMode.None,

        PerStep = TransactionMode.PerStep
    }
}
