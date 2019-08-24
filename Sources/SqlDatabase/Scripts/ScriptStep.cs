using System;
using System.Diagnostics;

namespace SqlDatabase.Scripts
{
    [DebuggerDisplay("{Script.DisplayName}")]
    public readonly struct ScriptStep
    {
        public ScriptStep(Version from, Version to, IScript script)
        {
            From = from;
            To = to;
            Script = script;
        }

        public Version From { get; }

        public Version To { get; }

        public IScript Script { get; }
    }
}