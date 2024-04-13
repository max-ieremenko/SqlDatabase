namespace SqlDatabase.CommandLine.Internal;

internal sealed class VariableArgBinder<TCommand> : IArgBinder<TCommand>
{
    private readonly Func<TCommand, Dictionary<string, string>> _getter;

    public VariableArgBinder(Func<TCommand, Dictionary<string, string>> getter)
    {
        _getter = getter;
    }

    public bool Match(Arg arg) =>
        arg.Key.StartsWith(ArgNames.Variable, StringComparison.OrdinalIgnoreCase) && arg.Key.Length > ArgNames.Variable.Length;

    public bool IsDuplicated(TCommand command, Arg arg) =>
        _getter(command).ContainsKey(GetVarName(arg.Key));

    public bool TryBind(TCommand command, Arg arg)
    {
        _getter(command).Add(GetVarName(arg.Key), arg.Value ?? string.Empty);
        return true;
    }

    public bool IsAssigned(TCommand command, out string key)
    {
        key = string.Empty;
        return true;
    }

    private static string GetVarName(string key) => key.Substring(ArgNames.Variable.Length);
}