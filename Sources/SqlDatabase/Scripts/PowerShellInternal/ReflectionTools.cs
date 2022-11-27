using System;
using System.Reflection;

namespace SqlDatabase.Scripts.PowerShellInternal;

internal static class ReflectionTools
{
    public static PropertyInfo FindProperty(this Type type, string name)
    {
        var result = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (result == null)
        {
            throw new InvalidOperationException(
                "public property {0} not found in {1}.".FormatWith(
                    name,
                    type.FullName));
        }

        return result;
    }

    public static EventInfo FindEvent(this Type type, string name)
    {
        var result = type.GetEvent(name, BindingFlags.Public | BindingFlags.Instance);
        if (result?.AddMethod == null || result.RemoveMethod == null)
        {
            throw new InvalidOperationException("Event {0} not found in {1}.".FormatWith(name, type.FullName));
        }

        return result;
    }

    public static T CreateDelegate<T>(this MethodInfo method)
        where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T));
    }
}