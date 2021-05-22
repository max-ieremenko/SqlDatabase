using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgramNetCore : ISqlDatabaseProgram
    {
        private readonly ICmdlet _owner;
        private readonly string _powerShellLocation;

        public SqlDatabaseProgramNetCore(ICmdlet owner)
        {
            _owner = owner;
            _powerShellLocation = Path.GetDirectoryName(typeof(PSCmdlet).Assembly.Location);
        }

        public void ExecuteCommand(GenericCommandLine command)
        {
            var logger = new CmdLetLogger(_owner);
            var args = new GenericCommandLineBuilder(command).BuildArray(false);

            // Fail to load configuration from [SqlDatabase.exe.config].
            // ---> An error occurred creating the configuration section handler for sqlDatabase: Could not load file or assembly 'SqlDatabase, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified.
            AssemblyLoadContext.Default.Resolving += AssemblyResolving;

            try
            {
                Program.Run(logger, args);
            }
            finally
            {
                AssemblyLoadContext.Default.Resolving -= AssemblyResolving;
            }
        }

        private Assembly AssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var token = assemblyName.GetPublicKeyToken();
            if (token == null || token.Length == 0)
            {
                var sqlDatabase = typeof(Program).Assembly;
                if (sqlDatabase.GetName().Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return sqlDatabase;
                }
            }

            var fileName = Path.Combine(_powerShellLocation, assemblyName.Name + ".dll");
            if (File.Exists(fileName))
            {
                return context.LoadFromAssemblyPath(fileName);
            }

            return null;
        }
    }
}
