namespace SqlDatabase.CommandLine.Internal;

internal static class ArgNames
{
    internal const char Sign = '-';
    internal const char Separator = '=';

    internal const string Help = "help";
    internal const string HelpShort = "h";

    internal const string Database = "database";
    internal const string Script = "from";
    internal const string InLineScript = "fromSql";
    internal const string Variable = "var";
    internal const string Configuration = "configuration";
    internal const string Transaction = "transaction";
    internal const string WhatIf = "whatIf";
    internal const string UsePowerShell = "usePowerShell";
    internal const string FolderAsModuleName = "folderAsModuleName";

    internal const string ExportToTable = "toTable";
    internal const string ExportToFile = "toFile";

    internal const string Log = "log";
}