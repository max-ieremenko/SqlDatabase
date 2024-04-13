namespace SqlDatabase.CommandLine;

public sealed class ExportCommandLine : ICommandLine
{
    public string Database { get; set; } = string.Empty;

    public List<ScriptSource> From { get; } = new();

    public string? DestinationTableName { get; set; }

    public string? DestinationFileName { get; set; }

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? Configuration { get; set; }

    public string? Log { get; set; }
}