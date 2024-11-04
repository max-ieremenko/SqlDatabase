namespace SqlDatabase.Adapter.MySql;

internal sealed class MySqlValueDataReader : IValueDataReader
{
    public object? Read(IDataReader source, int ordinal) => source[ordinal];
}