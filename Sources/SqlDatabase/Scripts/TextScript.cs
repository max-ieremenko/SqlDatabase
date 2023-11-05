using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SqlDatabase.Scripts;

internal sealed class TextScript : IScript
{
    public TextScript(string displayName, Func<Stream> readSqlContent, ISqlTextReader textReader)
    {
        DisplayName = displayName;
        ReadSqlContent = readSqlContent;
        TextReader = textReader;
    }

    public string DisplayName { get; set; }

    public Func<Stream> ReadSqlContent { get; internal set; }

    public ISqlTextReader TextReader { get; }

    public void Execute(IDbCommand? command, IVariables variables, ILogger logger)
    {
        var batches = ResolveBatches(variables, logger);

        if (command == null)
        {
            foreach (var batch in batches)
            {
                // what-if: just read to the end
            }

            return;
        }

        foreach (var batch in batches)
        {
            command.CommandText = batch;
            using (var reader = command.ExecuteReader())
            {
                var identOutput = false;
                do
                {
                    var columns = GetReaderColumns(reader);
                    if (columns == null)
                    {
                        ReadEmpty(reader);
                    }
                    else
                    {
                        ReadWithOutput(reader, columns, logger, identOutput);
                        identOutput = true;
                    }
                }
                while (reader.NextResult());
            }
        }
    }

    public IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger)
    {
        var batches = ResolveBatches(variables, logger);

        foreach (var batch in batches)
        {
            command.CommandText = batch;
            using (var reader = command.ExecuteReader())
            {
                yield return reader;
            }
        }
    }

    public IList<ScriptDependency> GetDependencies()
    {
        string? batch;
        using (var sql = ReadSqlContent())
        {
            batch = TextReader.ReadFirstBatch(sql);
        }

        if (string.IsNullOrWhiteSpace(batch))
        {
            return Array.Empty<ScriptDependency>();
        }

        return DependencyParser.ExtractDependencies(new StringReader(batch), DisplayName).ToArray();
    }

    private static string[]? GetReaderColumns(IDataReader reader)
    {
        using (var metadata = reader.GetSchemaTable())
        {
            // mssql: ColumnName is string.Empty if not defined
            // pgsql: ColumnName is DbNull if not defined
            return metadata
                ?.Rows
                .Cast<DataRow>()
                .OrderBy(i => (int)i["ColumnOrdinal"])
                .Select(i => i["ColumnName"].ToString()!)
                .ToArray();
        }
    }

    private static void ReadEmpty(IDataReader reader)
    {
        while (reader.Read())
        {
        }
    }

    private static void ReadWithOutput(IDataReader reader, string[] columns, ILogger logger, bool identOutput)
    {
        if (identOutput)
        {
            logger.Info(string.Empty);
        }

        var maxNameLength = 0;
        for (var i = 0; i < columns.Length; i++)
        {
            var columnName = columns[i];
            if (string.IsNullOrEmpty(columnName))
            {
                columnName = "(no name)";
            }

            columns[i] = columnName;
            maxNameLength = Math.Max(maxNameLength, columnName.Length);
        }

        logger.Info("output: " + string.Join("; ", columns));

        for (var i = 0; i < columns.Length; i++)
        {
            var name = columns[i];
            var spaces = maxNameLength - name.Length + 1;
            columns[i] = name + new string(' ', spaces) + ": ";
        }

        var rowsCount = 0;
        while (reader.Read())
        {
            rowsCount++;

            logger.Info("row " + rowsCount.ToString(CultureInfo.CurrentCulture));
            using (logger.Indent())
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    var value = reader.IsDBNull(i) ? "NULL" : Convert.ToString(reader.GetValue(i), CultureInfo.CurrentCulture);
                    logger.Info(columns[i] + value);
                }
            }
        }

        logger.Info("{0} row{1} selected".FormatWith(
            rowsCount.ToString(CultureInfo.CurrentCulture),
            rowsCount == 1 ? null : "s"));
    }

    private IEnumerable<string> ResolveBatches(IVariables variables, ILogger logger)
    {
        var scriptParser = new SqlScriptVariableParser(variables);

        var batches = new List<string>();
        using (var sql = ReadSqlContent())
        {
            foreach (var batch in TextReader.ReadBatches(sql))
            {
                var script = scriptParser.ApplyVariables(batch);
                if (!string.IsNullOrWhiteSpace(script))
                {
                    batches.Add(script);
                }
            }
        }

        var report = scriptParser.ValueByName.OrderBy(i => i.Key);
        foreach (var entry in report)
        {
            logger.Info("variable {0} was replaced with {1}".FormatWith(entry.Key, entry.Value));
        }

        return batches;
    }
}