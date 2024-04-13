using SqlDatabase.Adapter;

namespace SqlDatabase.Log;

internal sealed class CombinedLogger : ILogger, IDisposable
{
    private readonly ILogger? _logger1;
    private readonly ILogger? _logger2;

    public CombinedLogger(ILogger logger1, bool ownLogger1, ILogger logger2, bool ownLogger2)
    {
        _logger1 = logger1;
        _logger2 = logger2;
        OwnLogger1 = ownLogger1;
        OwnLogger2 = ownLogger2;
    }

    public bool OwnLogger1 { get; set; }

    public bool OwnLogger2 { get; set; }

    public void Error(string message)
    {
        _logger1?.Error(message);
        _logger2?.Error(message);
    }

    public void Info(string message)
    {
        _logger1?.Info(message);
        _logger2?.Info(message);
    }

    public IDisposable Indent()
    {
        return new IndentDisposable(_logger1?.Indent(), _logger2?.Indent());
    }

    public void Dispose()
    {
        if (OwnLogger1)
        {
            (_logger1 as IDisposable)?.Dispose();
        }

        if (OwnLogger2)
        {
            (_logger2 as IDisposable)?.Dispose();
        }
    }

    private sealed class IndentDisposable : IDisposable
    {
        private readonly IDisposable? _ident1;
        private readonly IDisposable? _ident2;

        public IndentDisposable(IDisposable? ident1, IDisposable? ident2)
        {
            _ident1 = ident1;
            _ident2 = ident2;
        }

        public void Dispose()
        {
            _ident1?.Dispose();
            _ident2?.Dispose();
        }
    }
}