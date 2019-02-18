using SqlDatabase.Log;

namespace SqlDatabase.PowerShell
{
    internal sealed class CmdLetLogger : LoggerBase
    {
        private readonly ICmdlet _owner;

        public CmdLetLogger(ICmdlet owner)
        {
            _owner = owner;
        }

        protected override void WriteError(string message)
        {
            _owner.WriteErrorLine(message);
        }

        protected override void WriteInfo(string message)
        {
            _owner.WriteLine(message);
        }
    }
}
