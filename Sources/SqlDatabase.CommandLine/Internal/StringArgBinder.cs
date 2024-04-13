namespace SqlDatabase.CommandLine.Internal;

internal sealed class StringArgBinder<TCommand> : IArgBinder<TCommand>
{
    private readonly string _key;
    private readonly Action<TCommand, string> _setter;
    private readonly bool _required;
    private bool _isAssigned;

    public StringArgBinder(string key, Action<TCommand, string> setter, bool required)
    {
        _key = key;
        _setter = setter;
        _required = required;
    }

    public bool Match(Arg arg) => arg.Is(_key);

    public bool IsDuplicated(TCommand command, Arg arg) => _isAssigned;

    public bool TryBind(TCommand command, Arg arg)
    {
        if (string.IsNullOrEmpty(arg.Value))
        {
            return false;
        }

        _isAssigned = true;
        _setter(command, arg.Value!);
        return true;
    }

    public bool IsAssigned(TCommand command, out string key)
    {
        key = _key;
        return !_required || _isAssigned;
    }
}