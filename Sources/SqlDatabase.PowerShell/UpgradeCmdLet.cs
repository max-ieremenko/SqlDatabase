using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsData.Update, "SqlDatabase")]
    [Alias(nameof(Command.Upgrade) + "-SqlDatabase")]
    public sealed class UpgradeCmdLet : SqlDatabaseCmdLet
    {
        public UpgradeCmdLet()
            : base(Command.Upgrade)
        {
        }
    }
}