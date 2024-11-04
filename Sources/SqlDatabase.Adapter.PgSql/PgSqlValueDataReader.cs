using Npgsql;
using SqlDatabase.Adapter.PgSql.UnmappedTypes;

namespace SqlDatabase.Adapter.PgSql;

internal sealed class PgSqlValueDataReader : IValueDataReader
{
    private readonly Dictionary<int, CompositeValueReader> _compositeColumns = new(0);

    public object? Read(IDataReader source, int ordinal) => Read((NpgsqlDataReader)source, ordinal);

    public object? Read(NpgsqlDataReader source, int ordinal)
    {
        if (_compositeColumns.TryGetValue(ordinal, out var composite))
        {
            if (source.IsDBNull(ordinal))
            {
                return null;
            }

            using (var nested = source.GetData(ordinal))
            {
                return composite.Read(nested);
            }
        }

        if (source.TryGetPrimitive(ordinal, out var result))
        {
            return result;
        }

        composite = new CompositeValueReader();
        using (var nested = source.GetData(ordinal))
        {
            result = composite.Read(nested);
        }

        _compositeColumns.Add(ordinal, composite);
        return result;
    }
}