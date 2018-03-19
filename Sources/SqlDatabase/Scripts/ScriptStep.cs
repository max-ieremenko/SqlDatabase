using System;

namespace SqlDatabase.Scripts
{
    public struct ScriptStep
    {
        public ScriptStep(Version from, Version to, IScript script)
        {
            From = from;
            To = to;
            Script = script;
        }

        public Version From { get; set; }

        public Version To { get; set; }

        public IScript Script { get; set; }
    }
}