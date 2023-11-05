using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SqlDatabase.Scripts.AssemblyInternal;

// public void Execute(SqlConnection connection)
internal sealed class ExecuteMethodResolverSqlConnection : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 1
               && typeof(SqlConnection) == parameters[0].ParameterType;
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method)
    {
        var execute = (Action<SqlConnection>)Delegate.CreateDelegate(
            typeof(Action<SqlConnection>),
            instance,
            method);

        return (command, _) => execute((SqlConnection)command.Connection!);
    }
}