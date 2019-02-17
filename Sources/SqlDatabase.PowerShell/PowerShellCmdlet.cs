using System;
using System.Management.Automation;

namespace SqlDatabase.PowerShell
{
    internal sealed class PowerShellCmdlet : ICmdlet
    {
        private readonly PSCmdlet _cmdlet;

        public PowerShellCmdlet(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void WriteErrorLine(string value)
        {
            _cmdlet.Host.UI.WriteErrorLine(value);
        }

        public void WriteLine(string value)
        {
            _cmdlet.Host.UI.WriteLine(value);
        }

        public void ThrowTerminatingError(ErrorCategory category, string message)
        {
            _cmdlet.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(message), null, category, null));
        }
    }
}