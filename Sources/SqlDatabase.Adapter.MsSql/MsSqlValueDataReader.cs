namespace SqlDatabase.Adapter.MsSql;

internal class MsSqlValueDataReader : IValueDataReader
{
    public object? Read(IDataReader source, int ordinal) => source[ordinal];
}