using System;
using System.IO;

namespace SqlDatabase.TestApi;

public sealed class TempConsoleOut : IDisposable
{
    private readonly TextWriter _originalOutput;
    private readonly Buffer _buffer;

    public TempConsoleOut()
    {
        _originalOutput = Console.Out;
        _buffer = new Buffer();
        Console.SetOut(_buffer);
    }

    public string GetOutput()
    {
        _buffer.Flush();
        return _buffer.ToString();
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
    }

    private sealed class Buffer : StringWriter, IDisposable
    {
        void IDisposable.Dispose()
        {
            Flush();
        }
    }
}