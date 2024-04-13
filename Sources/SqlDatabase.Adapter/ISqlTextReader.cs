namespace SqlDatabase.Adapter;

public interface ISqlTextReader
{
    string? ReadFirstBatch(Stream sql);

    IEnumerable<string> ReadBatches(Stream sql);
}