#if NET472
using System;
using System.Data;
using System.IO;

namespace SqlDatabase.Adapter.AssemblyScripts.Net472;

internal sealed class Net472SubDomain : ISubDomain
{
    private DomainDirectory? _appBase;
    private AppDomain? _app;
    private DomainAgent? _appAgent;

    public Net472SubDomain(ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        Logger = logger;
        AssemblyFileName = assemblyFileName;
        ReadAssemblyContent = readAssemblyContent;
    }

    public ILogger Logger { get; }

    public string AssemblyFileName { get; }

    public Func<byte[]> ReadAssemblyContent { get; }

    public void Initialize()
    {
        Logger.Info($"create domain for {AssemblyFileName}");

        var appBaseName = Path.GetFileName(AssemblyFileName);
        _appBase = new DomainDirectory(Logger);

        var entryAssembly = _appBase.SaveFile(ReadAssemblyContent(), appBaseName);

        var setup = new AppDomainSetup
        {
            ApplicationBase = _appBase.Location,
            ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
            LoaderOptimization = LoaderOptimization.MultiDomainHost
        };

        _app = AppDomain.CreateDomain(appBaseName, null, setup);
        _appAgent = (DomainAgent)_app.CreateInstanceFromAndUnwrap(GetType().Assembly.Location, typeof(DomainAgent).FullName);

        _appAgent.RedirectConsoleOut(new LoggerProxy(Logger));
        _appAgent.LoadAssembly(entryAssembly);
    }

    public void Unload()
    {
        _appAgent?.BeforeUnload();

        if (_app != null)
        {
            AppDomain.Unload(_app);
        }

        _app = null;
        _appAgent = null;
    }

    public bool ResolveScriptExecutor(string className, string methodName)
    {
        return _appAgent != null && _appAgent.ResolveScriptExecutor(className, methodName);
    }

    public bool Execute(IDbCommand command, IVariables variables)
    {
        return _appAgent!.Execute(command, new VariablesProxy(variables));
    }

    public void Dispose()
    {
        _appBase?.Dispose();
    }
}
#endif