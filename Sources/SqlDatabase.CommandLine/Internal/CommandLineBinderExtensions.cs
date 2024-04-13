using SqlDatabase.Adapter;

namespace SqlDatabase.CommandLine.Internal;

internal static class CommandLineBinderExtensions
{
    public static CommandLineBinder<T> BindDatabase<T>(this CommandLineBinder<T> binder, Action<T, string> setter)
    {
        binder.AddBinder(new StringArgBinder<T>(ArgNames.Database, setter, true));
        return binder;
    }

    public static CommandLineBinder<T> BindScripts<T>(this CommandLineBinder<T> binder, Func<T, List<ScriptSource>> getter)
    {
        binder.AddBinder(new ScriptSourceArgBinder<T>(getter));
        return binder;
    }

    public static CommandLineBinder<T> BindVariables<T>(this CommandLineBinder<T> binder, Func<T, Dictionary<string, string>> getter)
    {
        binder.AddBinder(new VariableArgBinder<T>(getter));
        return binder;
    }

    public static CommandLineBinder<T> BindConfiguration<T>(this CommandLineBinder<T> binder, Action<T, string> setter)
    {
        binder.AddBinder(new StringArgBinder<T>(ArgNames.Configuration, setter, false));
        return binder;
    }

    public static CommandLineBinder<T> BindLog<T>(this CommandLineBinder<T> binder, Action<T, string> setter)
    {
        binder.AddBinder(new StringArgBinder<T>(ArgNames.Log, setter, false));
        return binder;
    }

    public static CommandLineBinder<T> BindUsePowerShell<T>(this CommandLineBinder<T> binder, HostedRuntime runtime, Action<T, string> setter)
    {
        if (!runtime.IsPowershell && runtime.Version != FrameworkVersion.Net472)
        {
            binder.AddBinder(new StringArgBinder<T>(ArgNames.UsePowerShell, setter, false));
        }

        return binder;
    }

    public static CommandLineBinder<T> BindWhatIf<T>(this CommandLineBinder<T> binder, Action<T, bool> setter)
    {
        binder.AddBinder(new SwitchArgBinder<T>(ArgNames.WhatIf, setter));
        return binder;
    }

    public static CommandLineBinder<T> BindTransaction<T>(this CommandLineBinder<T> binder, Action<T, TransactionMode> setter)
    {
        binder.AddBinder(new EnumArgBinder<T, TransactionMode>(ArgNames.Transaction, setter));
        return binder;
    }

    public static CommandLineBinder<T> BindFolderAsModuleName<T>(this CommandLineBinder<T> binder, Action<T, bool> setter)
    {
        binder.AddBinder(new SwitchArgBinder<T>(ArgNames.FolderAsModuleName, setter));
        return binder;
    }

    public static CommandLineBinder<T> BindExportToTable<T>(this CommandLineBinder<T> binder, Action<T, string> setter)
    {
        binder.AddBinder(new StringArgBinder<T>(ArgNames.ExportToTable, setter, false));
        return binder;
    }

    public static CommandLineBinder<T> BindExportToFile<T>(this CommandLineBinder<T> binder, Action<T, string> setter)
    {
        binder.AddBinder(new StringArgBinder<T>(ArgNames.ExportToFile, setter, false));
        return binder;
    }
}