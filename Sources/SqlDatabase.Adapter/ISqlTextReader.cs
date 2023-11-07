using System.Collections.Generic;
using System.IO;

namespace SqlDatabase.Adapter;

public interface ISqlTextReader
{
    string? ReadFirstBatch(Stream sql);

    IEnumerable<string> ReadBatches(Stream sql);
}