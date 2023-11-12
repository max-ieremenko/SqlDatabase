using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Adapter.AssemblyScripts;

internal interface IEntryPoint
{
    bool Execute(IDbCommand command, IReadOnlyDictionary<string, string?> variables);
}