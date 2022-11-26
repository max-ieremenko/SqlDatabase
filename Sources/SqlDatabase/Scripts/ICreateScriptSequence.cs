using System.Collections.Generic;

namespace SqlDatabase.Scripts;

public interface ICreateScriptSequence
{
    IList<IScript> BuildSequence();
}