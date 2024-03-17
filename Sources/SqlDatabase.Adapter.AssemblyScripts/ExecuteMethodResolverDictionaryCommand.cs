using System.Reflection;

namespace SqlDatabase.Adapter.AssemblyScripts;

// public void Execute(IReadOnlyDictionary<string, string> variables, IDbCommand command)
internal sealed class ExecuteMethodResolverDictionaryCommand : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 2
               && typeof(IReadOnlyDictionary<string, string>) == parameters[0].ParameterType
               && typeof(IDbCommand) == parameters[1].ParameterType;
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method)
    {
        var execute = (Action<IReadOnlyDictionary<string, string?>, IDbCommand>)Delegate.CreateDelegate(
            typeof(Action<IReadOnlyDictionary<string, string?>, IDbCommand>),
            instance,
            method);

        return (command, variables) => execute(variables, command);
    }
}