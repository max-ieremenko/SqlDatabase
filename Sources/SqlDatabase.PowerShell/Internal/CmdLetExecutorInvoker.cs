using System.Reflection;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class CmdLetExecutorInvoker
{
    private const string ExecutorAssembly = "SqlDatabase.PowerShell.Internal";
    private const string ExecutorType = $"{ExecutorAssembly}.CmdLetExecutor";

    private readonly IDependencyResolver _dependencyResolver;
    private readonly Type _executorType;

    public CmdLetExecutorInvoker(IDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
        _executorType = ResolveExecutor(dependencyResolver);
    }

    private delegate void Run(IDictionary<string, object?> param, string currentDirectory, Action<string> writeInfo, Action<string> writeError);

    public void Invoke(CmdLetLogger logger, string workingDirectory, string methodName, IDictionary<string, object?> param)
    {
        _dependencyResolver.Attach();
        try
        {
            var method = (Run)ResolveMethod(methodName).CreateDelegate(typeof(Run));
            method(param, workingDirectory, logger.Info, logger.Error);
        }
        finally
        {
            _dependencyResolver.Detach();
        }
    }

    public string GetDefaultConfigurationFile()
    {
        _dependencyResolver.Attach();
        try
        {
            var method = (Func<string>)ResolveMethod(nameof(CmdLetExecutor.GetDefaultConfigurationFile)).CreateDelegate(typeof(Func<string>));
            return method();
        }
        finally
        {
            _dependencyResolver.Detach();
        }
    }

    private static Type ResolveExecutor(IDependencyResolver dependencyResolver)
    {
        dependencyResolver.Attach();
        try
        {
            var assembly = dependencyResolver.LoadDependency(ExecutorAssembly);
            if (assembly == null)
            {
                throw new InvalidOperationException($"Dependency {ExecutorAssembly}.dll not found");
            }

            return assembly.GetType(ExecutorType, throwOnError: true, ignoreCase: false);
        }
        finally
        {
            dependencyResolver.Detach();
        }
    }

    private MethodInfo ResolveMethod(string name)
    {
        var method = _executorType.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        return method ?? throw new InvalidOperationException($"Method {_executorType.FullName}.{name} not found");
    }
}