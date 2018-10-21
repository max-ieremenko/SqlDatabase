using System;
using System.Data;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.AssemblyInternal;

namespace SqlDatabase.Scripts
{
    internal sealed class AssemblyScript : IScript
    {
        public string DisplayName { get; set; }

        public Func<byte[]> ReadAssemblyContent { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            var assembly = ReadAssemblyContent();

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
                agent.LoadAssembly(assembly, logProxy);

                Execute(agent, command, variables, logProxy);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        internal static void Execute(
            DomainAgent agent,
            IDbCommand command,
            IVariables variables,
            ILogger logger)
        {
            var className = AppConfiguration.GetCurrent().AssemblyScript.ClassName;
            var methodName = AppConfiguration.GetCurrent().AssemblyScript.MethodName;

            if (!agent.ResolveScriptExecutor(logger, className, methodName))
            {
                throw new InvalidOperationException("Fail to resolve script executor.");
            }

            if (!agent.Execute(command, new VariablesProxy(variables)))
            {
                throw new InvalidOperationException("Errors during script execution.");
            }
        }
    }
}
