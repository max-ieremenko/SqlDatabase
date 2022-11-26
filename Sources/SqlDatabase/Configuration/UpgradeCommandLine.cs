using System;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PowerShellInternal;
using SqlDatabase.Scripts.UpgradeInternal;

namespace SqlDatabase.Configuration;

internal sealed class UpgradeCommandLine : CommandLineBase
{
    public TransactionMode Transaction { get; set; }

    public string UsePowerShell { get; set; }

    public bool FolderAsModuleName { get; set; }

    public bool WhatIf { get; set; }

    public override ICommand CreateCommand(ILogger logger)
    {
        var configuration = new ConfigurationManager();
        configuration.LoadFrom(ConfigurationFile);

        var database = CreateDatabase(logger, configuration, Transaction, WhatIf);
        var powerShellFactory = PowerShellFactory.Create(UsePowerShell);

        var sequence = new UpgradeScriptSequence
        {
            ScriptFactory = new ScriptFactory
            {
                AssemblyScriptConfiguration = configuration.SqlDatabase.AssemblyScript,
                PowerShellFactory = powerShellFactory,
                TextReader = database.Adapter.CreateSqlTextReader()
            },
            VersionResolver = new ModuleVersionResolver { Database = database, Log = logger },
            Sources = Scripts.ToArray(),
            Log = logger,
            FolderAsModuleName = FolderAsModuleName,
            WhatIf = WhatIf
        };

        return new DatabaseUpgradeCommand
        {
            Log = logger,
            Database = database,
            ScriptSequence = sequence,
            PowerShellFactory = powerShellFactory
        };
    }

    protected override bool ParseArg(Arg arg)
    {
        if (Arg.Transaction.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetTransaction(arg.Value);
            return true;
        }

        if (TryParseWhatIf(arg, out var value))
        {
            WhatIf = value;
            return true;
        }

#if NETCOREAPP || NET5_0_OR_GREATER
        if (Arg.UsePowerShell.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            UsePowerShell = arg.Value;
            return true;
        }
#endif

        if (TryParseSwitchParameter(arg, Arg.FolderAsModuleName, out value))
        {
            FolderAsModuleName = value;
            return true;
        }

        return false;
    }

    private void SetTransaction(string modeName)
    {
        if (!Enum.TryParse<TransactionMode>(modeName, true, out var mode))
        {
            throw new InvalidCommandLineException(Arg.Transaction, "Unknown transaction mode [{0}].".FormatWith(modeName));
        }

        Transaction = mode;
    }
}