using System;
using System.Globalization;
using System.IO;

namespace SqlDatabase.Log;

internal sealed class FileLogger : LoggerBase, IDisposable
{
    private readonly FileStream _file;
    private readonly StreamWriter _writer;

    public FileLogger(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _file = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        try
        {
            _file.Seek(0, SeekOrigin.End);
            _writer = new StreamWriter(_file);

            if (_file.Length > 0)
            {
                _writer.WriteLine();
                _writer.WriteLine();
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _file?.Dispose();
    }

    internal void Flush()
    {
        _writer.Flush();
    }

    protected override void WriteError(string message) => WriteLine("ERROR", message);

    protected override void WriteInfo(string message) => WriteLine("INFO", message);

    private void WriteLine(string type, string message)
    {
        _writer.Write(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fffff", CultureInfo.InvariantCulture));
        _writer.Write(" ");
        _writer.Write(type);
        _writer.Write(" ");
        _writer.WriteLine(message);
    }
}