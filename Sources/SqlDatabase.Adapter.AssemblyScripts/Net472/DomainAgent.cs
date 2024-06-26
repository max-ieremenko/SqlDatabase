﻿using System.Reflection;

namespace SqlDatabase.Adapter.AssemblyScripts.Net472;

internal sealed class DomainAgent : MarshalByRefObject
{
    private ConsoleListener? _consoleRedirect;

    internal Assembly? Assembly { get; set; }

    internal IEntryPoint? EntryPoint { get; set; }

    internal ILogger? Logger { get; set; }

    public void LoadAssembly(string fileName)
    {
        Assembly = Assembly.LoadFrom(fileName);
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    public void RedirectConsoleOut(TraceListener logger)
    {
        Logger = new LoggerProxy(logger);
        _consoleRedirect = new ConsoleListener(Logger);
    }

    public bool ResolveScriptExecutor(string className, string methodName)
    {
        // only for unit tests
        if (EntryPoint != null)
        {
            return true;
        }

        var resolver = new EntryPointResolver(Logger!, className, methodName);

        EntryPoint = resolver.Resolve(Assembly!);

        return EntryPoint != null;
    }

    public bool Execute(IDbCommand command, IReadOnlyDictionary<string, string?> variables)
    {
        return EntryPoint!.Execute(command, variables);
    }

    public void BeforeUnload()
    {
        _consoleRedirect?.Dispose();
        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
    }

    private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var argName = new AssemblyName(args.Name).Name;

        var sqlDataBase = GetType().Assembly!;
        if (sqlDataBase.GetName().Name!.Equals(argName, StringComparison.OrdinalIgnoreCase))
        {
            return sqlDataBase;
        }

        var fileName = Path.Combine(Path.GetDirectoryName(sqlDataBase.Location)!, argName + ".dll");
        if (File.Exists(fileName))
        {
            return Assembly.LoadFrom(fileName);
        }

        return null;
    }
}