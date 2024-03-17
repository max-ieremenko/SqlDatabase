namespace SqlDatabase.Configuration;

public sealed class DatabaseConfiguration
{
    internal const string PropertyGetCurrentVersionScript = "getCurrentVersion";
    internal const string PropertySetCurrentVersionScript = "setCurrentVersion";
    internal const string PropertyVariables = "variables";

    public string? GetCurrentVersionScript { get; set; }

    public string? SetCurrentVersionScript { get; set; }

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
}