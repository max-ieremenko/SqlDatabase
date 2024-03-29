using SqlDatabase.Adapter;
using SqlDatabase.Commands;

namespace SqlDatabase.Configuration;

internal sealed class CreateCommandLine : CommandLineBase
{
    public string? UsePowerShell { get; set; }

    public bool WhatIf { get; set; }

    public override ICommand CreateCommand(ILogger logger) => CreateCommand(logger, new EnvironmentBuilder(Runtime));

    internal ICommand CreateCommand(ILogger logger, IEnvironmentBuilder builder)
    {
        builder
            .WithLogger(logger)
            .WithConfiguration(ConfigurationFile)
            .WithPowerShellScripts(UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(Variables)
            .WithDataBase(ConnectionString!, TransactionMode.None, WhatIf);

        var database = builder.BuildDatabase();
        var scriptResolver = builder.BuildScriptResolver();
        var sequence = builder.BuildCreateSequence(Scripts);

        return new DatabaseCreateCommand(sequence, scriptResolver, database, logger);
    }

    protected override bool ParseArg(Arg arg)
    {
        if (Runtime.SupportUsePowerShell() && Arg.UsePowerShell.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            UsePowerShell = arg.Value;
            return true;
        }

        if (TryParseWhatIf(arg, out var value))
        {
            WhatIf = value;
            return true;
        }

        return false;
    }
}