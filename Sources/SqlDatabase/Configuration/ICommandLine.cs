using SqlDatabase.Adapter;
using SqlDatabase.Commands;

namespace SqlDatabase.Configuration;

internal interface ICommandLine
{
    void Parse(CommandLine args);

    ICommand CreateCommand(ILogger logger);
}