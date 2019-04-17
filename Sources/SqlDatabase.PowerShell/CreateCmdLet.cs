using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsCommon.New, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandCreate + "-SqlDatabase")]
    public sealed class CreateCmdLet : SqlDatabaseCmdLet
    {
        public CreateCmdLet()
            : base(CommandLineFactory.CommandCreate)
        {
        }
    }
}