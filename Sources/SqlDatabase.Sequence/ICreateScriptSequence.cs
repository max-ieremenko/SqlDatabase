using System.Collections.Generic;
using SqlDatabase.Adapter;

namespace SqlDatabase.Sequence;

public interface ICreateScriptSequence
{
    IList<IScript> BuildSequence();
}