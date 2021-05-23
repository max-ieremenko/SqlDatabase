using System.IO;
using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal
{
    internal abstract class PowerShellCommandBase
    {
        protected PowerShellCommandBase(PSCmdlet cmdlet)
        {
            Cmdlet = cmdlet;
        }

        public PSCmdlet Cmdlet { get; }

        // only for tests
        internal static ISqlDatabaseProgram Program { get; set; }

        public void Execute()
        {
            using (var resolver = DependencyResolverFactory.Create(Cmdlet))
            {
                resolver.Initialize();
                ExecuteCore();
            }
        }

        internal static void AppendDefaultConfiguration(GenericCommandLine command)
        {
            if (string.IsNullOrEmpty(command.ConfigurationFile))
            {
                var probingPath = Path.GetDirectoryName(typeof(PowerShellCommandBase).Assembly.Location);
                command.ConfigurationFile = ConfigurationManager.ResolveDefaultConfigurationFile(probingPath);
            }
        }

        protected abstract void BuildCommandLine(GenericCommandLineBuilder builder);

        private void ExecuteCore()
        {
            var builder = new GenericCommandLineBuilder();
            BuildCommandLine(builder);

            var command = builder.Build();
            AppendDefaultConfiguration(command);

            ResolveProgram().ExecuteCommand(command);
        }

        private ISqlDatabaseProgram ResolveProgram()
        {
            if (Program != null)
            {
                return Program;
            }

            return new SqlDatabaseProgram(new CmdLetLogger(Cmdlet));
        }
    }
}
