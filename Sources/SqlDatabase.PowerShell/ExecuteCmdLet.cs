using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandExecute + "-SqlDatabase")]
    public sealed class ExecuteCmdLet : SqlDatabaseCmdLet
    {
        public ExecuteCmdLet()
            : base(CommandLineFactory.CommandExecute)
        {
        }
    }
}