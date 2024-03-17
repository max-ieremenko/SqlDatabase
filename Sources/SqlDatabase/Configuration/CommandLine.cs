namespace SqlDatabase.Configuration;

internal readonly struct CommandLine
{
    public CommandLine(IList<Arg> args, string[]? original)
    {
        Args = args;
        Original = original;
    }

    public CommandLine(params Arg[] args)
    {
        Args = args;
        Original = null;
    }

    public IList<Arg> Args { get; }

    public string[]? Original { get; }
}