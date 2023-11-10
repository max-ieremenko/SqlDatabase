#if NET472 || NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis;

internal sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    public bool ReturnValue { get; }
}
#endif
