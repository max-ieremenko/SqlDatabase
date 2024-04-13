using SqlDatabase.Adapter;
using SqlDatabase.Adapter.Sql.Export;
using SqlDatabase.CommandLine;
using SqlDatabase.Configuration;
using SqlDatabase.FileSystem;
using SqlDatabase.Sequence;

namespace SqlDatabase.Commands;

internal sealed class CommandFactory
{
    private readonly ILogger _logger;
    private readonly IEnvironmentBuilder _builder;
    private readonly IFileSystemFactory _fileSystem;

    public CommandFactory(ILogger logger, IEnvironmentBuilder builder, IFileSystemFactory fileSystem)
    {
        _logger = logger;
        _builder = builder;
        _fileSystem = fileSystem;
    }

    public ICommand CreateCommand(ICommandLine commandLine)
    {
        if (commandLine is CreateCommandLine create)
        {
            return NewCreateCommand(create);
        }

        if (commandLine is ExecuteCommandLine execute)
        {
            return NewExecuteCommand(execute);
        }

        if (commandLine is UpgradeCommandLine upgrade)
        {
            return NewUpgradeCommand(upgrade);
        }

        if (commandLine is ExportCommandLine export)
        {
            return NewExportCommand(export);
        }

        throw new NotSupportedException();
    }

    private static Func<TextWriter> CreateExportOutput(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return static () => Console.Out;
        }

        return () => new StreamWriter(fileName, false);
    }

    private DatabaseCreateCommand NewCreateCommand(CreateCommandLine commandLine)
    {
        _builder
            .WithLogger(_logger)
            .WithConfiguration(commandLine.Configuration)
            .WithPowerShellScripts(commandLine.UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(commandLine.Variables)
            .WithDataBase(commandLine.Database, TransactionMode.None, commandLine.WhatIf);

        var database = _builder.BuildDatabase();
        var scriptResolver = _builder.BuildScriptResolver();
        var sequence = ToCreateScriptSequence(commandLine.From);

        return new DatabaseCreateCommand(sequence, scriptResolver, database, _logger);
    }

    private DatabaseExecuteCommand NewExecuteCommand(ExecuteCommandLine commandLine)
    {
        _builder
            .WithLogger(_logger)
            .WithConfiguration(commandLine.Configuration)
            .WithPowerShellScripts(commandLine.UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(commandLine.Variables)
            .WithDataBase(commandLine.Database, commandLine.Transaction, commandLine.WhatIf);

        var database = _builder.BuildDatabase();
        var scriptResolver = _builder.BuildScriptResolver();
        var sequence = ToCreateScriptSequence(commandLine.From);

        return new DatabaseExecuteCommand(sequence, scriptResolver, database, _logger);
    }

    private DatabaseUpgradeCommand NewUpgradeCommand(UpgradeCommandLine commandLine)
    {
        _builder
            .WithLogger(_logger)
            .WithConfiguration(commandLine.Configuration)
            .WithPowerShellScripts(commandLine.UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(commandLine.Variables)
            .WithDataBase(commandLine.Database, commandLine.Transaction, commandLine.WhatIf);

        var database = _builder.BuildDatabase();
        var scriptResolver = _builder.BuildScriptResolver();

        var from = ToFileSystem(commandLine.From);

        var sequence = new UpgradeScriptSequence(
            _builder.BuildScriptFactory(),
            database.GetCurrentVersion,
            from,
            _logger,
            commandLine.FolderAsModuleName,
            commandLine.WhatIf);

        return new DatabaseUpgradeCommand(sequence, scriptResolver, database, _logger);
    }

    private DatabaseExportCommand NewExportCommand(ExportCommandLine commandLine)
    {
        _builder
            .WithLogger(_logger)
            .WithConfiguration(commandLine.Configuration)
            .WithVariables(commandLine.Variables)
            .WithDataBase(commandLine.Database, TransactionMode.None, false);

        var database = _builder.BuildDatabase();
        var scriptResolver = _builder.BuildScriptResolver();
        var sequence = ToCreateScriptSequence(commandLine.From);

        var logger = string.IsNullOrEmpty(commandLine.DestinationFileName) ? new DataExportLogger(_logger) : _logger;

        return new DatabaseExportCommand(
            sequence,
            scriptResolver,
            CreateExportOutput(commandLine.DestinationFileName),
            database,
            logger)
        {
            DestinationTableName = commandLine.DestinationTableName
        };
    }

    private CreateScriptSequence ToCreateScriptSequence(List<ScriptSource> sources)
    {
        var from = ToFileSystem(sources);
        return new CreateScriptSequence(from, _builder.BuildScriptFactory());
    }

    private List<IFileSystemInfo> ToFileSystem(List<ScriptSource> sources)
    {
        var result = new List<IFileSystemInfo>(sources.Count);

        for (var i = 0; i < sources.Count; i++)
        {
            var source = sources[i];
            var script = source.IsInline
                ? _fileSystem.FromContent($"from{i + 1}.sql", source.Value)
                : _fileSystem.FileSystemInfoFromPath(source.Value);
            result.Add(script);
        }

        return result;
    }
}