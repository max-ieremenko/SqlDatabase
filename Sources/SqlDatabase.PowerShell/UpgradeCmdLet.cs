using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsData.Update, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandUpgrade + "-SqlDatabase")]
    public sealed class UpgradeCmdLet : SqlDatabaseCmdLet
    {
        public UpgradeCmdLet()
            : base(CommandLineFactory.CommandUpgrade)
        {
        }
    }
}