namespace SqlDatabase.CommandLine.Internal;

internal sealed class CommandLineBinder<TCommand>
{
    private readonly List<IArgBinder<TCommand>> _binders;
    private readonly TCommand _command;

    public CommandLineBinder(TCommand command)
    {
        _command = command;
        _binders = new();
    }

    public void AddBinder(IArgBinder<TCommand> binder) => _binders.Add(binder);

    public TCommand Build(IEnumerable<string> args)
    {
        foreach (var value in args)
        {
            if (!Arg.TryParse(value, out var arg))
            {
                throw new InvalidCommandLineException($"Invalid option [{value}].");
            }

            if (!TryFindBinder(arg, out var binder))
            {
                throw new InvalidCommandLineException($"Unknown option [{arg.Key}].");
            }

            if (binder.IsDuplicated(_command, arg))
            {
                throw new InvalidCommandLineException($"Option [{arg.Key}] is duplicated.");
            }

            if (!binder.TryBind(_command, arg))
            {
                throw new InvalidCommandLineException($"Fail to parse option [{value}].");
            }
        }

        for (var i = 0; i < _binders.Count; i++)
        {
            var binder = _binders[i];
            if (!binder.IsAssigned(_command, out var key))
            {
                throw new InvalidOperationException($"Option {key} is not specified.");
            }
        }

        return _command;
    }

    private bool TryFindBinder(Arg arg, [NotNullWhen(true)] out IArgBinder<TCommand>? binder)
    {
        for (var i = 0; i < _binders.Count; i++)
        {
            binder = _binders[i];
            if (binder.Match(arg))
            {
                return true;
            }
        }

        binder = null;
        return false;
    }
}