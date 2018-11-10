using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    internal sealed class DomainAgent : MarshalByRefObject
    {
        internal Assembly Assembly { get; set; }

        internal IEntryPoint EntryPoint { get; set; }

        internal ILogger Logger { get; set; }

        public void LoadAssembly(string fileName)
        {
            Assembly = Assembly.LoadFrom(fileName);
        }

        public void RedirectConsoleOut(TraceListener logger)
        {
            Logger = new LoggerProxy(logger);
            Console.SetOut(new ConsoleListener(Logger));
        }

        public bool ResolveScriptExecutor(string className, string methodName)
        {
            // only for unit tests
            if (EntryPoint != null)
            {
                return true;
            }

            var resolver = new EntryPointResolver
            {
                Log = Logger,
                ExecutorClassName = className,
                ExecutorMethodName = methodName
            };

            EntryPoint = resolver.Resolve(Assembly);

            return EntryPoint != null;
        }

        public bool Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            return EntryPoint.Execute(command, variables);
        }
    }
}
