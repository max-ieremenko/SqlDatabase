using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(nameof(Command.Upgrade), "SqlDatabase")]
    public sealed class UpgradeCmdLet : SqlDatabaseCmdLet
    {
        public UpgradeCmdLet()
            : base(Command.Upgrade)
        {
        }
    }
}