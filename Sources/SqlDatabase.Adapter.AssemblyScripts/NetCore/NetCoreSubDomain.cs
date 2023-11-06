#if !NET472
using System;
using System.Data;

namespace SqlDatabase.Adapter.AssemblyScripts.NetCore;

internal sealed class NetCoreSubDomain : ISubDomain
{
    private readonly AssemblyContext _assemblyContext;
    ////private ConsoleListener _consoleRedirect;

    public NetCoreSubDomain(ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        Logger = logger;
        AssemblyFileName = assemblyFileName;
        ReadAssemblyContent = readAssemblyContent;
        _assemblyContext = new AssemblyContext();
    }

    public ILogger Logger { get; set; }

    public string AssemblyFileName { get; set; }

    public Func<byte[]> ReadAssemblyContent { get; set; }

    private IEntryPoint? EntryPoint { get; set; }

    public void Initialize()
    {
        _assemblyContext.LoadScriptAssembly(ReadAssemblyContent());

        ////_consoleRedirect = new ConsoleListener(Logger);
    }

    public void Unload()
    {
        ////_consoleRedirect?.Dispose();

        _assemblyContext.UnloadAll();
    }

    public bool ResolveScriptExecutor(string className, string methodName)
    {
        if (_assemblyContext.ScriptAssembly == null)
        {
            return false;
        }

        var resolver = new EntryPointResolver(Logger, className, methodName);

        EntryPoint = resolver.Resolve(_assemblyContext.ScriptAssembly);

        return EntryPoint != null;
    }

    public bool Execute(IDbCommand command, IVariables variables)
    {
        return EntryPoint!.Execute(command, new VariablesProxy(variables));
    }

    public void Dispose()
    {
    }
}
#endif