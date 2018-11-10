using System;
using System.Data;
using System.IO;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.AssemblyInternal;

namespace SqlDatabase.Scripts
{
    internal sealed class AssemblyScript : IScript
    {
        public AssemblyScript()
        {
            AssemblyClassName = AppConfiguration.GetCurrent().AssemblyScript.ClassName;
            AssemblyMethodName = AppConfiguration.GetCurrent().AssemblyScript.MethodName;
        }

        public string DisplayName { get; set; }

        public Func<byte[]> ReadAssemblyContent { get; set; }

        internal string AssemblyClassName { get; set; }

        internal string AssemblyMethodName { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            logger.Info("create domain for {0}".FormatWith(DisplayName));

            var appBaseName = Path.GetFileName(DisplayName);
            using (var appBase = new DomainDirectory(logger))
            {
                var entryAssembly = appBase.SaveFile(ReadAssemblyContent(), appBaseName);

                var setup = new AppDomainSetup
                {
                    ApplicationBase = appBase.Location,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                };

                var domain = AppDomain.CreateDomain(appBaseName, null, setup);
                try
                {
                    var agent = (DomainAgent)domain.CreateInstanceFromAndUnwrap(GetType().Assembly.Location, typeof(DomainAgent).FullName);

                    agent.RedirectConsoleOut(new LoggerProxy(logger));
                    agent.LoadAssembly(entryAssembly);

                    Execute(agent, command, variables);
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
        }

        internal void Execute(
            DomainAgent agent,
            IDbCommand command,
            IVariables variables)
        {
            if (!agent.ResolveScriptExecutor(AssemblyClassName, AssemblyMethodName))
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
