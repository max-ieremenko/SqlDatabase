using System.Management.Automation;
using SqlDatabase.Log;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class CmdLetLogger : LoggerBase
{
    private readonly Cmdlet _cmdlet;

    public CmdLetLogger(Cmdlet cmdlet)
    {
        _cmdlet = cmdlet;
    }

    protected override void WriteError(string message)
    {
        _cmdlet.WriteError(new ErrorRecord(
            new InvalidOperationException(message),
            null,
            ErrorCategory.NotSpecified,
            null));
    }

    protected override void WriteInfo(string message)
    {
        _cmdlet.WriteInformation(new InformationRecord(message, null));
    }
}