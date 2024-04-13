namespace SqlDatabase.CommandLine.Internal;

internal readonly struct Arg
{
    public Arg(string key, string? value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }

    public string? Value { get; }

    public static bool TryParse(string? value, out Arg result)
    {
        value = value?.Trim();
        result = default;

        if (value == null || value.Length < 2 || value[0] != ArgNames.Sign)
        {
            return false;
        }

        value = value.Substring(1);

        var index = value.IndexOf(ArgNames.Separator);
        if (index < 0)
        {
            result = new Arg(value, null);
            return true;
        }

        if (index == 0)
        {
            return false;
        }

        var key = value.Substring(0, index).Trim();
        value = index == value.Length - 1 ? null : value.Substring(index + 1).Trim();
        result = new Arg(key, string.IsNullOrEmpty(value) ? null : value);
        return true;
    }

    public bool Is(string key) => Key.Equals(key, StringComparison.OrdinalIgnoreCase);

    public override string ToString()
    {
        if (Value != null)
        {
            return $"{Key}={Value}";
        }

        return Key;
    }
}