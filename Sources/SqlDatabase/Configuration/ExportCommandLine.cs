using SqlDatabase.Adapter;
using SqlDatabase.Adapter.Sql.Export;
using SqlDatabase.Commands;

namespace SqlDatabase.Configuration;

internal sealed class ExportCommandLine : CommandLineBase
{
    public string? DestinationTableName { get; set; }

    public string? DestinationFileName { get; set; }

    public override ICommand CreateCommand(ILogger logger) => CreateCommand(logger, new EnvironmentBuilder(Runtime));

    internal ICommand CreateCommand(ILogger logger, IEnvironmentBuilder builder)
    {
        builder
            .WithLogger(logger)
            .WithConfiguration(ConfigurationFile)
            .WithVariables(Variables)
            .WithDataBase(ConnectionString!, TransactionMode.None, false);

        var database = builder.BuildDatabase();
        var scriptResolver = builder.BuildScriptResolver();
        var sequence = builder.BuildCreateSequence(Scripts);

        return new DatabaseExportCommand(
            sequence,
            scriptResolver,
            CreateOutput(),
            database,
            WrapLogger(logger))
        {
            DestinationTableName = DestinationTableName
        };
    }

    internal Func<TextWriter> CreateOutput()
    {
        var fileName = DestinationFileName;

        if (string.IsNullOrEmpty(fileName))
        {
            return () => Console.Out;
        }

        return () => new StreamWriter(fileName, false);
    }

    internal ILogger WrapLogger(ILogger logger)
    {
        return string.IsNullOrEmpty(DestinationFileName) ? new DataExportLogger(logger) : logger;
    }

    protected override bool ParseArg(Arg arg)
    {
        if (Arg.ExportToTable.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            DestinationTableName = arg.Value;
            return true;
        }

        if (Arg.ExportToFile.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            DestinationFileName = arg.Value;
            return true;
        }

        if (Arg.InLineScript.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetInLineScript(arg.Value);
            return true;
        }

        return false;
    }
}