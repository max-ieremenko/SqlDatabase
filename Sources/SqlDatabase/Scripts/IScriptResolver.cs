using SqlDatabase.Adapter;

namespace SqlDatabase.Scripts;

internal interface IScriptResolver
{
    void InitializeEnvironment(ILogger logger, IEnumerable<IScript> scripts);
}