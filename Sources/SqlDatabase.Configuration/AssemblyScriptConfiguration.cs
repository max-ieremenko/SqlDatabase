namespace SqlDatabase.Configuration;

public sealed class AssemblyScriptConfiguration
{
    internal const string PropertyClassName = "className";
    internal const string PropertyMethodName = "methodName";

    public string? ClassName { get; set; }

    public string? MethodName { get; set; }
}