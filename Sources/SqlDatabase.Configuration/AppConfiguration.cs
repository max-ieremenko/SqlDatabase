namespace SqlDatabase.Configuration;

public sealed class AppConfiguration
{
    public const string SectionName = "sqlDatabase";

    internal const string PropertyGetCurrentVersionScript = "getCurrentVersion";
    internal const string PropertySetCurrentVersionScript = "setCurrentVersion";
    internal const string PropertyAssemblyScript = "assemblyScript";
    internal const string PropertyVariables = "variables";
    internal const string PropertyMsSql = "mssql";
    internal const string PropertyPgSql = "pgsql";
    internal const string PropertyMySql = "mysql";

    public string? GetCurrentVersionScript { get; set; }

    public string? SetCurrentVersionScript { get; set; }

    public AssemblyScriptConfiguration AssemblyScript { get; } = new();

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public DatabaseConfiguration MsSql { get; } = new();

    public DatabaseConfiguration PgSql { get; } = new();

    public DatabaseConfiguration MySql { get; } = new();
}