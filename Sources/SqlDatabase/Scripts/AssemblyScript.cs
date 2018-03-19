using System;
using System.Data;

namespace SqlDatabase.Scripts
{
    internal sealed partial class AssemblyScript : IScript
    {
        public const string ExecutorClassName = "SqlDatabaseScript";
        public const string ExecutorMethodName = "Execute";

        public string DisplayName { get; set; }

        public byte[] Assembly { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };

            logger.Info("create domain for {0}".FormatWith(DisplayName));
            var domain = AppDomain.CreateDomain(DisplayName, null, setup);
            try
            {
                var agent = (DomainAgent)domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, typeof(DomainAgent).FullName);
                var logProxy = new LoggerProxy(logger);

                agent.RedirectConsoleOut(logProxy);
                agent.LoadAssembly(Assembly, logProxy);

                if (agent.ResolveScriptExecutor(logProxy))
                {
                    agent.Execute(command, new VariablesProxy(variables));
                }
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }
}
