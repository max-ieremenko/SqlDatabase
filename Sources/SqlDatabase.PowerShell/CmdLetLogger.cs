using System.Management.Automation;
using SqlDatabase.Log;

namespace SqlDatabase.PowerShell
{
    internal sealed class CmdLetLogger : LoggerBase
    {
        private readonly PSCmdlet _owner;

        public CmdLetLogger(PSCmdlet owner)
        {
            _owner = owner;
        }

        protected override void WriteError(string message)
        {
            _owner.Host.UI.WriteErrorLine(message);
        }

        protected override void WriteInfo(string message)
        {
            _owner.Host.UI.WriteLine(message);
        }
    }
}
