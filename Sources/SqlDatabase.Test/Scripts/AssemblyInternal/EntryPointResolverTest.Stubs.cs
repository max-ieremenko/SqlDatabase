using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlDatabase.Scripts.AssemblyInternal;

public partial class EntryPointResolverTest
{
    public sealed class ExampleSqlDatabaseScript
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DatabaseScriptWithInvalidConstructor
    {
        public DatabaseScriptWithInvalidConstructor()
        {
            throw new NotSupportedException();
        }

        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DatabaseScriptWithOneParameter
    {
        public void ExecuteCommand(IDbCommand command)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DatabaseScriptWithConnection
    {
        public void Run(SqlConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}