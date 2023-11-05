using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlDatabase.Scripts.AssemblyInternal;

internal sealed class EntryPointResolver
{
    public EntryPointResolver(ILogger log, string executorClassName, string executorMethodName)
    {
        Log = log;
        ExecutorClassName = executorClassName;
        ExecutorMethodName = executorMethodName;
    }

    public ILogger Log { get; }

    public string ExecutorClassName { get; internal set; }

    public string ExecutorMethodName { get; internal set; }

    public IEntryPoint? Resolve(Assembly assembly)
    {
        Log.Info("resolve script executor");

        var type = ResolveClass(assembly);
        if (type == null)
        {
            return null;
        }

        var method = ResolveMethod(type);

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
            scriptInstance = Activator.CreateInstance(type)!;
        }
        catch (Exception ex)
        {
            Log.Error("Fail to create instance of {0}.".FormatWith(type.FullName), ex);
            return null;
        }

        return new DefaultEntryPoint(Log, scriptInstance, method.Resolver.CreateDelegate(scriptInstance, method.Method));
    }

    private Type? ResolveClass(Assembly assembly)
    {
        var filter = assembly
            .GetExportedTypes()
            .Where(i => i.IsClass && !i.IsAbstract && !i.IsGenericTypeDefinition);

        if (ExecutorClassName.IndexOf('.') >= 0 || ExecutorClassName.IndexOf('+') >= 0)
        {
            filter = filter.Where(i => i.FullName?.EndsWith(ExecutorClassName, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            filter = filter.Where(i => ExecutorClassName.Equals(i.Name, StringComparison.Ordinal));
        }

        var candidates = filter.ToList();
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
                        return new ExecuteMethodRef(priority, i, resolver);
                    }
                }

                return null;
            })
            .Where(i => i != null)
            .OrderBy(i => i!.Priority)
            .ToList();

        if (methods.Count == 0)
        {
            Log.Error("public void {0}(IDbCommand command, IReadOnlyDictionary<string, string> variables) not found in {1}.".FormatWith(ExecutorMethodName, type));
        }

        return methods[0]!;
    }

    private sealed class ExecuteMethodRef
    {
        public ExecuteMethodRef(int priority, MethodInfo method, ExecuteMethodResolverBase resolver)
        {
            Priority = priority;
            Method = method;
            Resolver = resolver;
        }

        public int Priority { get; }

        public MethodInfo Method { get; }

        public ExecuteMethodResolverBase Resolver { get; }
    }
}