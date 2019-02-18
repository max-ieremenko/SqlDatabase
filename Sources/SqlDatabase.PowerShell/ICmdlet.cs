using System.Management.Automation;

namespace SqlDatabase.PowerShell
{
    internal interface ICmdlet
    {
        void WriteErrorLine(string value);

        void WriteLine(string value);

        void ThrowTerminatingError(ErrorCategory category, string message);
    }
}
