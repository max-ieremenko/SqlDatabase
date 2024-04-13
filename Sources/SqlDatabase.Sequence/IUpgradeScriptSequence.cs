namespace SqlDatabase.Sequence;

public interface IUpgradeScriptSequence
{
    IList<ScriptStep> BuildSequence();
}