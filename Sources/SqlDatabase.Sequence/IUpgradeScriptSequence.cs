using System.Collections.Generic;

namespace SqlDatabase.Sequence;

public interface IUpgradeScriptSequence
{
    IList<ScriptStep> BuildSequence();
}