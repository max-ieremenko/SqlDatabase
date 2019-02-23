using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsCommon.New, "SqlDatabase")]
    [Alias(nameof(Command.Create) + "-SqlDatabase")]
    public sealed class CreateCmdLet : SqlDatabaseCmdLet
    {
        public CreateCmdLet()
            : base(Command.Create)
        {
        }
    }
}