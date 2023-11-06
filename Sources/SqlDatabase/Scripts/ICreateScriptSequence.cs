using System.Collections.Generic;
using SqlDatabase.Adapter;

namespace SqlDatabase.Scripts;

public interface ICreateScriptSequence
{
    IList<IScript> BuildSequence();
}