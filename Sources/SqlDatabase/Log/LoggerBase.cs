using System;

namespace SqlDatabase.Log;

internal abstract class LoggerBase : ILogger
{
    private string _indentation;

    public void Error(string message)
    {
        WriteError(message);
    }

    public void Info(string message)
    {
        WriteInfo(_indentation + message);
    }

    public IDisposable Indent()
    {
        const int IndentValue = 3;
        const char IndentChar = ' ';

        _indentation += new string(IndentChar, IndentValue);
        return new DisposableAction(() =>
        {
            var length = (_indentation.Length / IndentValue) - 1;
            _indentation = length == 0 ? null : new string(IndentChar, length * IndentValue);
        });
    }

    protected abstract void WriteError(string message);

    protected abstract void WriteInfo(string message);
}