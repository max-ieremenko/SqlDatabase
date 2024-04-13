namespace SqlDatabase.CommandLine.Internal;

internal sealed class SwitchArgBinder<TCommand> : IArgBinder<TCommand>
{
    private readonly string _key;
    private readonly Action<TCommand, bool> _setter;
    private bool _isAssigned;

    public SwitchArgBinder(string key, Action<TCommand, bool> setter)
    {
        _key = key;
        _setter = setter;
    }

    public bool Match(Arg arg) => arg.Is(_key);

    public bool IsDuplicated(TCommand command, Arg arg) => _isAssigned;

    public bool TryBind(TCommand command, Arg arg)
    {
        var value = true;
        if (!string.IsNullOrEmpty(arg.Value) && !bool.TryParse(arg.Value, out value))
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