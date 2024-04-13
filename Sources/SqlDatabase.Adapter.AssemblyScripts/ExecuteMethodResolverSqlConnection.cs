using System.Linq.Expressions;
using System.Reflection;

namespace SqlDatabase.Adapter.AssemblyScripts;

// public void Execute(SqlConnection connection)
internal sealed class ExecuteMethodResolverSqlConnection : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 1
               && "System.Data.SqlClient.SqlConnection".Equals(parameters[0].ParameterType.FullName, StringComparison.Ordinal);
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method)
    {
        var command = Expression.Parameter(typeof(IDbCommand), "command");
        var variables = Expression.Parameter(typeof(IReadOnlyDictionary<string, string?>), "variables");

        var connection = Expression.Property(command, nameof(IDbCommand.Connection));
        var sqlConnection = Expression.Convert(connection, method.GetParameters()[0].ParameterType);
        var call = Expression.Call(Expression.Constant(instance), method, sqlConnection);

        return Expression.Lambda<Action<IDbCommand, IReadOnlyDictionary<string, string?>>>(call, command, variables).Compile();
    }
}