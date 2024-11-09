using SqlDatabase.Log;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class CmdLetLogger : LoggerBase
{
    private readonly Action<string> _writeInfo;
    private readonly Action<string> _writeError;

    public CmdLetLogger(Action<string> writeInfo, Action<string> writeError)
    {
        _writeInfo = writeInfo;
        _writeError = writeError;
    }

    protected override void WriteError(string message) => _writeError(message);

    protected override void WriteInfo(string message) => _writeInfo(message);
}