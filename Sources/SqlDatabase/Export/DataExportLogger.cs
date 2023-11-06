using System;
using System.IO;
using System.Text;
using SqlDatabase.Adapter;

namespace SqlDatabase.Export;

internal sealed class DataExportLogger : ILogger
{
    private readonly ILogger _origin;

    public DataExportLogger(ILogger origin)
    {
        _origin = origin;
    }

    public void Error(string message)
    {
        var escaped = new StringBuilder(message.Length);

        using (var reader = new StringReader(message))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (escaped.Length > 0)
                {
                    escaped.AppendLine();
                }

                escaped.Append("-- ").Append(line);
            }
        }

        _origin.Error(escaped.ToString());
    }

    public void Info(string? message)
    {
        // ignore
    }

    public IDisposable Indent()
    {
        return _origin.Indent();
    }
}