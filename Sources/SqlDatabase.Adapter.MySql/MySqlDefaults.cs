namespace SqlDatabase.Adapter.MySql;

internal static class MySqlDefaults
{
    public const string DefaultSelectVersion = "SELECT version FROM version WHERE module_name = 'database'";
    public const string DefaultUpdateVersion = "UPDATE version SET version='{{TargetVersion}}' WHERE module_name = 'database'";
}