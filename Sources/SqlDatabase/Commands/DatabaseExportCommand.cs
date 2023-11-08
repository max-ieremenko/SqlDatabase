using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SqlDatabase.Adapter;
using SqlDatabase.Adapter.Sql.Export;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;

namespace SqlDatabase.Commands;

internal sealed class DatabaseExportCommand : DatabaseCommandBase
{
    public DatabaseExportCommand(
        ICreateScriptSequence scriptSequence,
        IScriptResolver scriptResolver,
        Func<TextWriter> openOutput,
        IDatabase database,
        ILogger log)
        : base(database, log)
    {
        ScriptSequence = scriptSequence;
        ScriptResolver = scriptResolver;
        OpenOutput = openOutput;
    }

    public ICreateScriptSequence ScriptSequence { get; }

    public IScriptResolver ScriptResolver { get; }

    public Func<TextWriter> OpenOutput { get; }

    public string? DestinationTableName { get; set; }

    internal Func<IDataExporter> ExporterFactory { get; set; } = () => new DataExporter();

    protected override void Greet(string databaseLocation)
    {
        Log.Info($"Export data from {databaseLocation}");
    }

    protected override void ExecuteCore()
    {
        var sequences = ScriptSequence.BuildSequence();
        if (sequences.Count == 0)
        {
            return;
        }

        ScriptResolver.InitializeEnvironment(Log, sequences);

        using (var output = OpenOutput())
        {
            var readerIndex = 0;

            var exporter = ExporterFactory();
            exporter.Output = Database.Adapter.CreateSqlWriter(output);
            exporter.Log = Log;

            if (string.IsNullOrWhiteSpace(DestinationTableName))
            {
                DestinationTableName = exporter.Output.GetDefaultTableName();
            }

            foreach (var script in sequences)
            {
                var timer = Stopwatch.StartNew();
                Log.Info($"export {script.DisplayName} ...");

                using (Log.Indent())
                {
                    ExportScript(exporter, script, ref readerIndex);
                }

                Log.Info($"done in {timer.Elapsed}");
            }
        }
    }

    private static string GetExportTableName(string? name, int index, int subIndex)
    {
        var result = new StringBuilder(20);

        if (string.IsNullOrWhiteSpace(name))
        {
            result.Append("dbo.SqlDatabaseExport");
        }
        else
        {
            result.Append(name);
        }

        if (index > 1)
        {
            result.Append(index);
        }

        if (subIndex > 1)
        {
            result.Append('_').Append(subIndex);
        }

        return result.ToString();
    }

    private void ExportScript(IDataExporter exporter, IScript script, ref int readerIndex)
    {
        foreach (var reader in Database.ExecuteReader(script))
        {
            readerIndex++;
            var readerSubIndex = 0;

            do
            {
                readerSubIndex++;
                exporter.Export(reader, GetExportTableName(DestinationTableName, readerIndex, readerSubIndex));
            }
            while (reader.NextResult());
        }
    }
}