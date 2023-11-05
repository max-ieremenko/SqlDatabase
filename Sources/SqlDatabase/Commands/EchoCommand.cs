using SqlDatabase.Configuration;

namespace SqlDatabase.Commands;

internal sealed class EchoCommand : ICommand
{
    public EchoCommand(ILogger logger, CommandLine args)
    {
        Logger = logger;
        Args = args;
    }

    public ILogger Logger { get; }

    public CommandLine Args { get; }

    public void Execute()
    {
        if (Args.Original != null)
        {
            foreach (var arg in Args.Original)
            {
                Logger.Info(arg);
            }
        }
    }
}