using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    internal sealed class EntryPointResolver
    {
        public ILogger Log { get; set; }

        public string ExecutorClassName { get; set; }

        public string ExecutorMethodName { get; set; }

        public IEntryPoint Resolve(Assembly assembly)
        {
            Log.Info("resolve script executor");

            var type = ResolveClass(assembly);
            if (type == null)
            {
                return null;
            }

            var method = ResolveMethod(type);
            if (method == null)
            {
                return null;
            }

            var message = new StringBuilder()
                .AppendFormat("found {0}.{1}(", type.FullName, method.Name);
            var args = method.GetParameters();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    message.Append(", ");
                }

                message
                    .Append(args[i].ParameterType.Name)
                    .Append(" ")
                    .Append(args[i].Name);
            }

            message.Append(")");
            Log.Info(message.ToString());

            object scriptInstance;
            try
            {
                scriptInstance = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Log.Error("Fail to create instance of {0}: {1}".FormatWith(type.FullName, ex.Message));
                Log.Info(ex.ToString());
                return null;
            }

            var execute = (Action<IDbCommand, IReadOnlyDictionary<string, string>>)Delegate.CreateDelegate(
                typeof(Action<IDbCommand, IReadOnlyDictionary<string, string>>),
                scriptInstance,
                method);

            return new DefaultEntryPoint
            {
                Log = Log,
                ScriptInstance = scriptInstance,
                Method = execute
            };
        }

        private Type ResolveClass(Assembly assembly)
        {
            var candidates = assembly
                .GetExportedTypes()
                .Where(i => i.IsClass)
                .Where(i => ExecutorClassName.Equals(i.Name, StringComparison.Ordinal))
                .ToList();

            if (candidates.Count == 0)
            {
                Log.Error("public class {0} not found.".FormatWith(ExecutorClassName));
                return null;
            }

            if (candidates.Count != 1)
            {
                Log.Error("There are {0} items with signature public class {1}.".FormatWith(candidates.Count, ExecutorClassName));
                return null;
            }

            return candidates[0];
        }

        private MethodInfo ResolveMethod(Type type)
        {
            var method = type
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
                Log.Error("public void {0}(IDbCommand command, IReadOnlyDictionary<string, string> variables) not found in {1}.".FormatWith(ExecutorMethodName, type));
            }

            return method;
        }
    }
}
