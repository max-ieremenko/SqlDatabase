#if !NET452
using System;
using System.Data;

namespace SqlDatabase.Scripts.AssemblyInternal.NetCore
{
    internal sealed class NetCoreSubDomain : ISubDomain
    {
        private readonly AssemblyContext _assemblyContext = new AssemblyContext();
        private ConsoleListener _consoleRedirect;

        public ILogger Logger { get; set; }

        public string AssemblyFileName { get; set; }

        public Func<byte[]> ReadAssemblyContent { get; set; }

        private IEntryPoint EntryPoint { get; set; }

        public void Initialize()
        {
            _assemblyContext.LoadScriptAssembly(ReadAssemblyContent());

            _consoleRedirect = new ConsoleListener(Logger);
        }

        public void Unload()
        {
            _consoleRedirect?.Dispose();

            _assemblyContext.UnloadAll();
        }

        public bool ResolveScriptExecutor(string className, string methodName)
        {
            var resolver = new EntryPointResolver
            {
                Log = Logger,
                ExecutorClassName = className,
                ExecutorMethodName = methodName
            };

            EntryPoint = resolver.Resolve(_assemblyContext.ScriptAssembly);

            return EntryPoint != null;
        }

        public bool Execute(IDbCommand command, IVariables variables)
        {
            return EntryPoint.Execute(command, new VariablesProxy(variables));
        }

        public void Dispose()
        {
        }
    }
}
#endif