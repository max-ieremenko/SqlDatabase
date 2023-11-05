using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Scripts.AssemblyInternal;

internal interface IEntryPoint
{
    bool Execute(IDbCommand command, IReadOnlyDictionary<string, string?> variables);
}