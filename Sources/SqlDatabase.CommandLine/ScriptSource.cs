namespace SqlDatabase.CommandLine;

public readonly struct ScriptSource : IEquatable<ScriptSource>
{
    public ScriptSource(bool isInline, string value)
    {
        IsInline = isInline;
        Value = value;
    }

    public bool IsInline { get; }

    public string Value { get; }

    public bool Equals(ScriptSource other) =>
        IsInline == other.IsInline && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ScriptSource other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;
}