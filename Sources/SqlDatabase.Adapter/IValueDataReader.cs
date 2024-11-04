namespace SqlDatabase.Adapter;

public interface IValueDataReader
{
    object? Read(IDataReader source, int ordinal);
}