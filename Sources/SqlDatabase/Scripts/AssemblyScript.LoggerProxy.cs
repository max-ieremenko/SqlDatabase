using System;

namespace SqlDatabase.Scripts
{
    internal partial class AssemblyScript
    {
        private sealed class LoggerProxy : MarshalByRefObject, ILogger
        {
            private readonly ILogger _log;

            public LoggerProxy(ILogger log)
            {
                _log = log;
            }

            public void Error(string message)
            {
                _log.Error(message);
            }

            public void Info(string message)
            {
                _log.Info(message);
            }
        }
    }
}
