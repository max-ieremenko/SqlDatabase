using System.Collections.Generic;
using System.IO;

namespace SqlDatabase.Scripts
{
    internal interface ISqlTextReader
    {
        IEnumerable<string> Read(Stream sql);
    }
}
