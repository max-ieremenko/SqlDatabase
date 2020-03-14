﻿using System;
using System.Reflection;
using System.Runtime.Loader;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgramNetCore : ISqlDatabaseProgram
    {
        private readonly ICmdlet _owner;

        public SqlDatabaseProgramNetCore(ICmdlet owner)
        {
            _owner = owner;
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

            return null;
        }
    }
}
