namespace SqlDatabase.CommandLine.Internal;

internal sealed class EnumArgBinder<TCommand, TEnum> : IArgBinder<TCommand>
    where TEnum : struct
{
    private readonly string _key;
    private readonly Action<TCommand, TEnum> _setter;
    private bool _isAssigned;

    public EnumArgBinder(string key, Action<TCommand, TEnum> setter)
    {
        _key = key;
        _setter = setter;
    }

    public bool Match(Arg arg) => arg.Is(_key);

    public bool IsDuplicated(TCommand command, Arg arg) => _isAssigned;

    public bool TryBind(TCommand command, Arg arg)
    {
        if (string.IsNullOrEmpty(arg.Value) || !Enum.TryParse<TEnum>(arg.Value, true, out var value))
        {
            return false;
        }

        _isAssigned = true;
        _setter(command, value);
        return true;
    }

    public bool IsAssigned(TCommand command, out string key)
    {
        key = _key;
        return true;
    }
}