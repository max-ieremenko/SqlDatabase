#if NET452
using System;
using System.Data;
using System.IO;

namespace SqlDatabase.Scripts.AssemblyInternal.Net452
{
    internal sealed class Net452SubDomain : ISubDomain
    {
        private DomainDirectory _appBase;
        private AppDomain _app;
        private DomainAgent _appAgent;

        public ILogger Logger { get; set; }

        public string AssemblyFileName { get; set; }

        public Func<byte[]> ReadAssemblyContent { get; set; }

        public void Initialize()
        {
            Logger.Info("create domain for {0}".FormatWith(AssemblyFileName));

            var appBaseName = Path.GetFileName(AssemblyFileName);
            _appBase = new DomainDirectory(Logger);

            var entryAssembly = _appBase.SaveFile(ReadAssemblyContent(), appBaseName);

            var setup = new AppDomainSetup
            {
                ApplicationBase = _appBase.Location,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };

            _app = AppDomain.CreateDomain(appBaseName, null, setup);
            _appAgent = (DomainAgent)_app.CreateInstanceFromAndUnwrap(GetType().Assembly.Location, typeof(DomainAgent).FullName);

            _appAgent.RedirectConsoleOut(new LoggerProxy(Logger));
            _appAgent.LoadAssembly(entryAssembly);
        }

        public void Unload()
        {
            if (_app != null)
            {
                AppDomain.Unload(_app);
            }

            _app = null;
            _appAgent = null;
        }

        public bool ResolveScriptExecutor(string className, string methodName)
        {
            return _appAgent.ResolveScriptExecutor(className, methodName);
        }

        public bool Execute(IDbCommand command, IVariables variables)
        {
            return _appAgent.Execute(command, new VariablesProxy(variables));
        }

        public void Dispose()
        {
            _appBase?.Dispose();
        }
    }
}
#endif