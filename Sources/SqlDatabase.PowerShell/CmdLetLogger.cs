using System;
using System.Management.Automation;

namespace SqlDatabase.PowerShell
{
    internal sealed class CmdLetLogger : ILogger
    {
        private readonly Cmdlet _owner;

        public CmdLetLogger(Cmdlet owner)
        {
            _owner = owner;
        }

        public void Error(string message)
        {
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }
    }
}
