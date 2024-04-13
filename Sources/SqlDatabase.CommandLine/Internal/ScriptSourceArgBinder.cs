namespace SqlDatabase.CommandLine.Internal;

internal sealed class ScriptSourceArgBinder<TCommand> : IArgBinder<TCommand>
{
    private readonly Func<TCommand, List<ScriptSource>> _getter;

    public ScriptSourceArgBinder(Func<TCommand, List<ScriptSource>> getter)
    {
        _getter = getter;
    }

    public bool Match(Arg arg) => arg.Is(ArgNames.Script) || arg.Is(ArgNames.InLineScript);

    public bool IsDuplicated(TCommand command, Arg arg) => false;

    public bool TryBind(TCommand command, Arg arg)
    {
        if (!string.IsNullOrEmpty(arg.Value))
        {
            _getter(command).Add(new ScriptSource(arg.Is(ArgNames.InLineScript), arg.Value!));
        }

        return true;
    }

    public bool IsAssigned(TCommand command, out string key)
    {
        key = ArgNames.Script;
        return _getter(command).Count > 0;
    }
}