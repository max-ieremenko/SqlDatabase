namespace SqlDatabase.CommandLine.Internal;

internal interface IArgBinder<TCommand>
{
    bool Match(Arg arg);

    bool IsDuplicated(TCommand command, Arg arg);

    bool TryBind(TCommand command, Arg arg);

    bool IsAssigned(TCommand command, [NotNullWhen(false)] out string? key);
}