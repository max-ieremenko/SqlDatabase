using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(nameof(Command.Execute), "SqlDatabase")]
    public sealed class ExecuteCmdLet : SqlDatabaseCmdLet
    {
        public ExecuteCmdLet()
            : base(Command.Execute)
        {
        }
    }
}