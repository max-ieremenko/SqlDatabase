using System.Reflection;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class ReflectionTools
{
    public static PropertyInfo FindProperty(this Type type, string name)
    {
        var result = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (result == null)
        {
            throw new InvalidOperationException($"public property {name} not found in {type.FullName}.");
        }

        return result;
    }

    public static MethodInfo FindStaticMethod(this Type type, string name, params Type[] parameters)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (var i = 0; i < methods.Length; i++)
        {
            var method = methods[i];
            if (!name.Equals(method.Name, StringComparison.Ordinal))
            {
                continue;
            }

            var input = method.GetParameters();
            if (input.Length != parameters.Length)
            {
                continue;
            }

            var inputMatch = true;
            for (var j = 0; j < parameters.Length; j++)
            {
                if (parameters[i] != input[i].ParameterType)
                {
                    inputMatch = false;
                    break;
                }
            }

            if (inputMatch)
            {
                return method;
            }
        }

        throw new InvalidOperationException($"public static {name} not found in {type.FullName}.");
    }

    public static EventInfo FindEvent(this Type type, string name)
    {
        var result = type.GetEvent(name, BindingFlags.Public | BindingFlags.Instance);
        if (result?.AddMethod == null || result.RemoveMethod == null)
        {
            throw new InvalidOperationException($"Event {name} not found in {type.FullName}.");
        }

        return result;
    }

    public static T CreateDelegate<T>(this MethodInfo method)
        where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T));
    }
}