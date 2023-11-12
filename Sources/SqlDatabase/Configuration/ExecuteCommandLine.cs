using System;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;

namespace SqlDatabase.Configuration;

internal sealed class ExecuteCommandLine : CommandLineBase
{
    public TransactionMode Transaction { get; set; }

    public string? UsePowerShell { get; set; }

    public bool WhatIf { get; set; }

    public override ICommand CreateCommand(ILogger logger) => CreateCommand(logger, new EnvironmentBuilder());

    internal ICommand CreateCommand(ILogger logger, IEnvironmentBuilder builder)
    {
        builder
            .WithLogger(logger)
            .WithConfiguration(ConfigurationFile)
            .WithPowerShellScripts(UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(Variables)
            .WithDataBase(ConnectionString!, Transaction, WhatIf);

        var database = builder.BuildDatabase();
        var scriptResolver = builder.BuildScriptResolver();
        var sequence = builder.BuildCreateSequence(Scripts);

        return new DatabaseExecuteCommand(sequence, scriptResolver, database, logger);
    }

    protected override bool ParseArg(Arg arg)
    {
        if (Arg.Transaction.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetTransaction(arg.Value);
            return true;
        }

        if (Arg.InLineScript.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetInLineScript(arg.Value);
            return true;
        }

#if NET5_0_OR_GREATER
        if (Arg.UsePowerShell.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
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

    private void SetTransaction(string? modeName)
    {
        if (!Enum.TryParse<TransactionMode>(modeName, true, out var mode))
        {
            throw new InvalidCommandLineException(Arg.Transaction, $"Unknown transaction mode [{modeName}].");
        }

        Transaction = mode;
    }
}