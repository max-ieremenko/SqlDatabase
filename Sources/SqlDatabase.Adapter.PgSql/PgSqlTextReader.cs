﻿using System.Text.RegularExpressions;

namespace SqlDatabase.Adapter.PgSql;

internal sealed class PgSqlTextReader : ISqlTextReader
{
    private const int MaxFirstBatchSize = 20;
    private readonly Regex _semicolonRegex = new Regex("^(\\s*;+\\s*)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string? ReadFirstBatch(Stream sql)
    {
        var script = new StringBuilder();

        using (var reader = new StreamReader(sql))
        {
            string? line;
            var lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (script.Length == 0 && string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                lineNumber++;
                if (_semicolonRegex.IsMatch(line) || lineNumber > MaxFirstBatchSize)
                {
                    break;
                }

                if (script.Length > 0)
                {
                    script.AppendLine();
                }

                script.Append(line);
            }
        }

        return script.ToString();
    }

    public IEnumerable<string> ReadBatches(Stream sql)
    {
        string script;
        using (var reader = new StreamReader(sql))
        {
            script = reader.ReadToEnd();
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            return Array.Empty<string>();
        }

        return new[] { script };
    }
}