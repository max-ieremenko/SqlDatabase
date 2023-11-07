namespace SqlDatabase.Adapter.MsSql;

internal static class MsSqlDefaults
{
    public const string DefaultSelectVersion = "SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)";
    public const string DefaultUpdateVersion = "EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'";
}