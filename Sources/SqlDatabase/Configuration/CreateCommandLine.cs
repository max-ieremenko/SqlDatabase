using System.Linq;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Configuration;

internal sealed class CreateCommandLine : CommandLineBase
{
    public string? UsePowerShell { get; set; }

    public bool WhatIf { get; set; }

    public override ICommand CreateCommand(ILogger logger)
    {
        var configuration = new ConfigurationManager();
        configuration.LoadFrom(ConfigurationFile);

        var powerShellFactory = PowerShellFactory.Create(UsePowerShell);
        var database = CreateDatabase(logger, configuration, TransactionMode.None, WhatIf);

        var sequence = new CreateScriptSequence(
            Scripts.ToArray(),
            new ScriptFactory(configuration.SqlDatabase.AssemblyScript, powerShellFactory, database.Adapter.CreateSqlTextReader()));

        return new DatabaseCreateCommand(sequence, powerShellFactory, database, logger);
    }

    protected override bool ParseArg(Arg arg)
    {
#if NET5_0_OR_GREATER
        if (Arg.UsePowerShell.Equals(arg.Key, System.StringComparison.OrdinalIgnoreCase))
        {
            UsePowerShell = arg.Value;
            return true;
        }
#endif

        if (TryParseWhatIf(arg, out var value))
        {
            WhatIf = value;
            return true;
        }

        return false;
    }
}