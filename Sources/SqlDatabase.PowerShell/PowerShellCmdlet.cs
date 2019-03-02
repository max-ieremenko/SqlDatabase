using System;
using System.Management.Automation;

namespace SqlDatabase.PowerShell
{
    internal sealed class PowerShellCmdlet : ICmdlet
    {
        private readonly PSCmdlet _cmdlet;

        public PowerShellCmdlet(PSCmdlet cmdlet)
        {
            ////var errorActionPreference = cmdlet.SessionState.PSVariable.GetValue("ErrorActionPreference");
            ////var informationPreference = cmdlet.SessionState.PSVariable.GetValue("InformationPreference");
            ////var warningPreference = cmdlet.SessionState.PSVariable.GetValue("WarningPreference");

            _cmdlet = cmdlet;
        }

        public void WriteErrorLine(string value)
        {
            _cmdlet.WriteError(new ErrorRecord(
                new InvalidOperationException(value),
                null,
                ErrorCategory.NotSpecified,
                null));
        }

        public void WriteLine(string value)
        {
            _cmdlet.WriteInformation(new InformationRecord(value, null));
        }
    }
}