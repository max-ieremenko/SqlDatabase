using Npgsql;

namespace SqlDatabase.Adapter.PgSql.UnmappedTypes;

internal sealed class CompositeValueReader
{
    private readonly Dictionary<int, CompositeValueReader> _compositeColumns = new(0);
    private string[]? _names;

    public Composite? Read(NpgsqlNestedDataReader source)
    {
        Composite? result = null;

        while (source.Read())
        {
            var names = GetNames(source);

            if (result == null)
            {
                result = new Composite(names);
            }

            var length = names.Length;
            var row = new object?[length];
            result.Rows.Add(row);

            for (var i = 0; i < length; i++)
            {
                row[i] = ReadValue(source, i);
            }
        }

        return result;
    }

    private string[] GetNames(NpgsqlNestedDataReader source)
    {
        if (_names == null)
        {
            var count = source.FieldCount;
            _names = new string[count];
            for (var i = 0; i < count; i++)
            {
                _names[i] = source.GetName(i);
            }
        }

        return _names;
    }

    private object? ReadValue(NpgsqlNestedDataReader source, int ordinal)
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

        using (var nested = source.GetData(ordinal))
        {
            composite = new CompositeValueReader();
            result = composite.Read(nested);
            _compositeColumns.Add(ordinal, composite);
            return result;
        }
    }
}