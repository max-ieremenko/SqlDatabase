namespace SqlDatabase.Adapter.AssemblyScripts.Net472;

internal sealed class LoggerProxy : TraceListener, ILogger
{
    private readonly TraceListener? _output;
    private readonly ILogger? _input;

    public LoggerProxy(ILogger input)
    {
        _input = input;
    }

    public LoggerProxy(TraceListener output)
    {
        _output = output;
    }

    public override void Write(string? message)
    {
        throw new NotSupportedException();
    }

    public override void WriteLine(string? message)
    {
        _input?.Info(message ?? string.Empty);
    }

    public override void Fail(string? message)
    {
        _input?.Error(message!);
    }

    void ILogger.Error(string message)
    {
        _output?.Fail(message);
    }

    void ILogger.Info(string? message)
    {
        _output?.WriteLine(message);
    }

    IDisposable ILogger.Indent()
    {
        throw new NotSupportedException();
    }
}