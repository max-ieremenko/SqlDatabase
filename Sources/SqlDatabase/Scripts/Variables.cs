using System;
using System.Collections.Generic;
using SqlDatabase.Adapter;

namespace SqlDatabase.Scripts;

internal sealed class Variables : IVariables
{
    private readonly IDictionary<string, VariableValue> _valueByName = new Dictionary<string, VariableValue>(StringComparer.OrdinalIgnoreCase);

    public string? DatabaseName
    {
        get => GetValue(nameof(DatabaseName));
        set
        {
            SetValue(VariableSource.Runtime, nameof(DatabaseName), value);
            SetValue(VariableSource.Runtime, "DbName", value);
        }
    }

    public string? CurrentVersion
    {
        get => GetValue(nameof(CurrentVersion));
        set => SetValue(VariableSource.Runtime, nameof(CurrentVersion), value);
    }

    public string? TargetVersion
    {
        get => GetValue(nameof(TargetVersion));
        set => SetValue(VariableSource.Runtime, nameof(TargetVersion), value);
    }

    public string? ModuleName
    {
        get => GetValue(nameof(ModuleName));
        set => SetValue(VariableSource.Runtime, nameof(ModuleName), value);
    }

    public IEnumerable<string> GetNames()
    {
        return _valueByName.Keys;
    }

    public string? GetValue(string name)
    {
        if (_valueByName.TryGetValue(name, out var value))
        {
            if (value.Source != VariableSource.ConfigurationFile)
            {
                return value.Value;
            }
        }

        var environmentValue = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(environmentValue) ? value.Value : environmentValue;
    }

    internal void SetValue(VariableSource source, string name, string? value)
    {
        if (!_valueByName.TryGetValue(name, out var oldValue)
            || source <= oldValue.Source)
        {
            if (value == null)
            {
                _valueByName.Remove(name);
            }
            else
            {
                _valueByName[name] = new VariableValue(source, value);
            }
        }
    }

    private readonly struct VariableValue
    {
        public VariableValue(VariableSource source, string value)
        {
            Source = source;
            Value = value;
        }

        public VariableSource Source { get; }

        public string Value { get; }

        public override bool Equals(object? obj) => throw new NotSupportedException();

        public override int GetHashCode() => throw new NotSupportedException();
    }
}