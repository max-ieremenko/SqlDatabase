using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class SqlDatabaseProgram : ISqlDatabaseProgram
{
    private readonly ILogger _logger;
    private readonly string _currentDirectory;

    public SqlDatabaseProgram(ILogger logger, string currentDirectory)
    {
        _logger = logger;
        _currentDirectory = currentDirectory;
    }

    public void ExecuteCommand(ICommandLine command)
    {
        Program.RunPowershell(_logger, command, _currentDirectory);
    }
}