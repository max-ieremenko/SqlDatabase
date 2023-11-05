using SqlDatabase.Commands;

namespace SqlDatabase.Configuration;

internal sealed class EchoCommandLine : ICommandLine
{
    private CommandLine _args;

    public void Parse(CommandLine args)
    {
        _args = args;
    }

    public ICommand CreateCommand(ILogger logger) => new EchoCommand(logger, _args);
}