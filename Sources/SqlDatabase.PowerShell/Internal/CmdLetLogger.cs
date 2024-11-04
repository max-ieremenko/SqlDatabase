using System.Management.Automation;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class CmdLetLogger
{
    private readonly Cmdlet _cmdlet;

    public CmdLetLogger(Cmdlet cmdlet)
    {
        _cmdlet = cmdlet;
        Info = WriteInfo;
        Error = WriteError;
    }

    public Action<string> Info { get; }

    public Action<string> Error { get; }

    private void WriteError(string message) => _cmdlet.WriteError(new ErrorRecord(
        new InvalidOperationException(message),
        null,
        ErrorCategory.NotSpecified,
        null));

    private void WriteInfo(string message) => _cmdlet.WriteInformation(new InformationRecord(message, null));
}