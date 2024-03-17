namespace SqlDatabase.Adapter;

public interface IScript
{
    string DisplayName { get; set; }

    void Execute(IDbCommand? command, IVariables variables, ILogger logger);

    IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger);

    TextReader? GetDependencies();
}