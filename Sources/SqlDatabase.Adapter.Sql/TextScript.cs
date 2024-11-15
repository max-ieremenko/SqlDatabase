﻿using System.Globalization;

namespace SqlDatabase.Adapter.Sql;

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

    public TextReader? GetDependencies()
    {
        string? batch;
        using (var sql = ReadSqlContent())
        {
            batch = TextReader.ReadFirstBatch(sql);
        }

        if (string.IsNullOrWhiteSpace(batch))
        {
            return null;
        }

        return new StringReader(batch);
    }

    private static string[]? GetReaderColumns(IDataReader reader)
    {
        using (var metadata = reader.GetSchemaTable())
        {
            // mssql: ColumnName is string.Empty if not defined
            // pgsql: ColumnName is ?column?
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

        var s = rowsCount == 1 ? null : "s";
        logger.Info($"{rowsCount} row{s} selected");
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
            logger.Info($"variable {entry.Key} was replaced with {entry.Value}");
        }

        return batches;
    }
}