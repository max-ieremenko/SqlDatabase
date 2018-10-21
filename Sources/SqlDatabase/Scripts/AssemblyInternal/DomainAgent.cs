using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    internal sealed class DomainAgent : MarshalByRefObject
    {
        internal Assembly Assembly { get; set; }

        internal IEntryPoint EntryPoint { get; set; }

        public void LoadAssembly(byte[] content, ILogger logger)
        {
            logger.Info("load assembly");

            Assembly = System.Reflection.Assembly.Load(content);
        }

        public void RedirectConsoleOut(ILogger logger)
        {
            Console.SetOut(new ConsoleListener(logger));
        }

        public bool ResolveScriptExecutor(ILogger logger, string className, string methodName)
        {
            // only for unit tests
            if (EntryPoint != null)
            {
                return true;
            }

            var resolver = new EntryPointResolver
            {
                Log = logger,
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
