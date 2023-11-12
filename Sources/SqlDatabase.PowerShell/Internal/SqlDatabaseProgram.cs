using SqlDatabase.Adapter;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class SqlDatabaseProgram : ISqlDatabaseProgram
{
    private readonly ILogger _logger;

    public SqlDatabaseProgram(ILogger logger)
    {
        _logger = logger;
    }

    public void ExecuteCommand(GenericCommandLine command)
    {
        var args = new GenericCommandLineBuilder(command).BuildArray();
        Program.Run(_logger, args);
    }
}