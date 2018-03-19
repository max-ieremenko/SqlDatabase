using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SqlDatabase.Scripts
{
    internal partial class AssemblyScript
    {
        private sealed class DomainAgent : MarshalByRefObject
        {
            private Assembly _assembly;
            private object _scriptInstance;
            private Action<IDbCommand, IReadOnlyDictionary<string, string>> _execute;

            public void LoadAssembly(byte[] content, ILogger logger)
            {
                logger.Info("load assembly");

                _assembly = System.Reflection.Assembly.Load(content);
            }

            public void RedirectConsoleOut(ILogger logger)
            {
                Console.SetOut(new ConsoleListener(logger));
            }

            public bool ResolveScriptExecutor(ILogger logger)
            {
                logger.Info("resolve script executor");

                var candidates = _assembly
                    .GetExportedTypes()
                    .Where(i => i.IsClass)
                    .Where(i => i.IsPublic)
                    .Where(i => ExecutorClassName.Equals(i.Name, StringComparison.Ordinal))
                    .ToList();

                if (candidates.Count == 0)
                {
                    logger.Error("public class {0} not found.".FormatWith(ExecutorClassName));
                    return false;
                }

                if (candidates.Count != 1)
                {
                    logger.Error("There are {0} items with signature public class {1}.".FormatWith(candidates.Count, ExecutorClassName));
                    return false;
                }

                var method = candidates[0]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(i => i.ReturnType == typeof(void))
                    .Where(i => ExecutorMethodName.Equals(i.Name, StringComparison.OrdinalIgnoreCase))
                    .Where(i =>
                    {
                        var p = i.GetParameters();
                        return p.Length == 2
                               && typeof(IDbCommand) == p[0].ParameterType
                               && typeof(IReadOnlyDictionary<string, string>) == p[1].ParameterType;
                    })
                    .FirstOrDefault();

                if (method == null)
                {
                    logger.Error("public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables) not found in {0}.".FormatWith(candidates[0]));
                    return false;
                }

                _scriptInstance = Activator.CreateInstance(candidates[0]);
                _execute = (Action<IDbCommand, IReadOnlyDictionary<string, string>>)Delegate.CreateDelegate(
                    typeof(Action<IDbCommand, IReadOnlyDictionary<string, string>>),
                    _scriptInstance,
                    method);

                return true;
            }

            public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
            {
                try
                {
                    _execute(command, variables);
                }
                finally
                {
                    (_scriptInstance as IDisposable)?.Dispose();
                }
            }
        }
    }
}
