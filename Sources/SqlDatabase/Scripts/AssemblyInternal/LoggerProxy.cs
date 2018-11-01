using System;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    internal sealed class LoggerProxy : MarshalByRefObject, ILogger
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

        public IDisposable Indent()
        {
            return new RemoteDisposable(_log.Indent());
        }

        private sealed class RemoteDisposable : MarshalByRefObject, IDisposable
        {
            private readonly IDisposable _obj;

            public RemoteDisposable(IDisposable obj)
            {
                _obj = obj;
            }

            public void Dispose()
            {
                _obj?.Dispose();
            }
        }
    }
}
