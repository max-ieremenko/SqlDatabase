﻿using System.Dynamic;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal sealed class VariablesProxy : DynamicObject
{
    private readonly IVariables _variables;

    public VariablesProxy(IVariables variables)
    {
        _variables = variables;
    }

    public override bool TryGetMember(
        GetMemberBinder binder,
        [NotNullWhen(true)] out object? result)
    {
        result = _variables.GetValue(binder.Name);
        return result != null;
    }
}