using System.Collections.Generic;
using SqlDatabase.Scripts;

namespace SqlDatabase
{
    public interface ICreateScriptSequence
    {
        IList<IScript> BuildSequence();
    }
}