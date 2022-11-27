using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SqlDatabase.Scripts.AssemblyInternal;

// public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
internal sealed class ExecuteMethodResolverCommandDictionary : ExecuteMethodResolverBase
{
    public override bool IsMatch(MethodInfo method)
    {
        var parameters = method.GetParameters();
        return parameters.Length == 2
               && typeof(IDbCommand) == parameters[0].ParameterType
               && typeof(IReadOnlyDictionary<string, string>) == parameters[1].ParameterType;
    }

    public override Action<IDbCommand, IReadOnlyDictionary<string, string>> CreateDelegate(object instance, MethodInfo method)
    {
        return (Action<IDbCommand, IReadOnlyDictionary<string, string>>)Delegate.CreateDelegate(
            typeof(Action<IDbCommand, IReadOnlyDictionary<string, string>>),
            instance,
            method);
    }
}