using System;
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
                .AppendFormat("found {0}.{1}(", type.FullName, method.Method.Name);
            var args = method.Method.GetParameters();
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

            return new DefaultEntryPoint
            {
                Log = Log,
                ScriptInstance = scriptInstance,
                Method = method.Resolver.CreateDelegate(scriptInstance, method.Method)
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

        private ExecuteMethodRef ResolveMethod(Type type)
        {
            var methodResolvers = new ExecuteMethodResolverBase[]
            {
                new ExecuteMethodResolverCommandDictionary(),
                new ExecuteMethodResolverDictionaryCommand(),
                new ExecuteMethodResolverCommand(),
                new ExecuteMethodResolverDbConnection(),
                new ExecuteMethodResolverSqlConnection()
            };

            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(i => i.ReturnType == typeof(void))
                .Where(i => ExecutorMethodName.Equals(i.Name, StringComparison.OrdinalIgnoreCase))
                .Select(i =>
                {
                    for (var priority = 0; priority < methodResolvers.Length; priority++)
                    {
                        var resolver = methodResolvers[priority];
                        if (resolver.IsMatch(i))
                        {
                            return new ExecuteMethodRef
                            {
                                Method = i,
                                Resolver = resolver,
                                Priority = priority
                            };
                        }
                    }

                    return null;
                })
                .Where(i => i != null)
                .OrderBy(i => i.Priority)
                .ToList();

            if (methods.Count == 0)
            {
                Log.Error("public void {0}(IDbCommand command, IReadOnlyDictionary<string, string> variables) not found in {1}.".FormatWith(ExecutorMethodName, type));
            }

            return methods[0];
        }

        private sealed class ExecuteMethodRef
        {
            public int Priority { get; set; }

            public MethodInfo Method { get; set; }

            public ExecuteMethodResolverBase Resolver { get; set; }
        }
    }
}
