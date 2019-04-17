using SqlDatabase.Configuration;

namespace SqlDatabase.Commands
{
    internal sealed class EchoCommand : ICommand
    {
        public ILogger Logger { get; set; }

        public CommandLine Args { get; set; }

        public void Execute()
        {
            foreach (var arg in Args.Original)
            {
                Logger.Info(arg);
            }
        }
    }
}
