using System.Collections.Generic;
using System.IO;

namespace SqlDatabase.Scripts;

internal interface ISqlTextReader
{
    string? ReadFirstBatch(Stream sql);

    IEnumerable<string> ReadBatches(Stream sql);
}