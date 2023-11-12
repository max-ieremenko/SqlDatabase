namespace SqlDatabase.Adapter.PgSql;

internal static class PgSqlDefaults
{
    public const string DefaultSelectVersion = "SELECT version FROM public.version WHERE module_name = 'database'";
    public const string DefaultUpdateVersion = "UPDATE public.version SET version='{{TargetVersion}}' WHERE module_name = 'database'";
}