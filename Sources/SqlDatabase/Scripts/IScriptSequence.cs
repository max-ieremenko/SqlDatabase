using System;
using System.Collections.Generic;

namespace SqlDatabase.Scripts
{
    public interface IScriptSequence
    {
        IList<ScriptStep> BuildSequence(Version currentVersion);
    }
}