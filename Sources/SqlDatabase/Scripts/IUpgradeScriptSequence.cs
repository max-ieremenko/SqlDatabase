using System;
using System.Collections.Generic;

namespace SqlDatabase.Scripts
{
    public interface IUpgradeScriptSequence
    {
        IList<ScriptStep> BuildSequence(Version currentVersion);
    }
}