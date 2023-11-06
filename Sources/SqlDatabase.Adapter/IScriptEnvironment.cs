namespace SqlDatabase.Adapter;

public interface IScriptEnvironment
{
    bool IsSupported(IScript script);

    void Initialize(ILogger logger);
}