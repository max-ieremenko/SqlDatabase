using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SqlDatabase.Scripts.AssemblyInternal;

// public void Execute(IDbCommand command)
internal sealed class ExecuteMethodResolverCommand : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 1
               && typeof(IDbCommand) == parameters[0].ParameterType;
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method)
    {
        var execute = (Action<IDbCommand>)Delegate.CreateDelegate(
            typeof(Action<IDbCommand>),
            instance,
            method);

        return (command, variables) => execute(command);
    }
}