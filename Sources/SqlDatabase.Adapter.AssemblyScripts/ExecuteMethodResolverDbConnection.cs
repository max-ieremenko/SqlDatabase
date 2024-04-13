using System.Reflection;

namespace SqlDatabase.Adapter.AssemblyScripts;

// public void Execute(IDbConnection connection)
internal sealed class ExecuteMethodResolverDbConnection : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 1
               && typeof(IDbConnection) == parameters[0].ParameterType;
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method)
    {
        var execute = (Action<IDbConnection>)Delegate.CreateDelegate(
            typeof(Action<IDbConnection>),
            instance,
            method);

        return (command, _) => execute(command.Connection!);
    }
}