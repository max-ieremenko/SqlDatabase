using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Scripts;

public interface IScript
{
    string DisplayName { get; set; }

    void Execute(IDbCommand? command, IVariables variables, ILogger logger);

    IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger);

    IList<ScriptDependency> GetDependencies();
}