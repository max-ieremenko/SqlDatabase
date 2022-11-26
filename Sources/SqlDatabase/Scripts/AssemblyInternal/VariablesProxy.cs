using System;
using System.Collections;
using System.Collections.Generic;

namespace SqlDatabase.Scripts.AssemblyInternal;

internal sealed class VariablesProxy : MarshalByRefObject, IReadOnlyDictionary<string, string>
{
    private readonly IVariables _variables;

    public VariablesProxy(IVariables variables)
    {
        _variables = variables;
    }

    public int Count => throw new NotSupportedException();

    public IEnumerable<string> Keys => throw new NotSupportedException();

    public IEnumerable<string> Values => throw new NotSupportedException();

    public string this[string key] => _variables.GetValue(key);

    public bool ContainsKey(string key)
    {
        return _variables.GetValue(key) != null;
    }

    public bool TryGetValue(string key, out string value)
    {
        value = _variables.GetValue(key);
        return value != null;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}