using SqlDatabase.Adapter;

namespace SqlDatabase.Sequence;

[DebuggerDisplay("{Script.DisplayName}")]
public readonly struct ScriptStep
{
    public ScriptStep(string moduleName, Version from, Version to, IScript script)
    {
        ModuleName = moduleName;
        From = from;
        To = to;
        Script = script;
    }

    public string ModuleName { get; }

    public Version From { get; }

    public Version To { get; }

    public IScript Script { get; }
}