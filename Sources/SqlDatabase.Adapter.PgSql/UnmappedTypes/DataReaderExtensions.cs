using System.Data.Common;

namespace SqlDatabase.Adapter.PgSql.UnmappedTypes;

internal static class DataReaderExtensions
{
    public static bool TryGetPrimitive(this DbDataReader reader, int ordinal, out object? value)
    {
        if (reader.IsDBNull(ordinal))
        {
            value = null;
            return true;
        }

        try
        {
            value = reader.GetValue(ordinal);
            return true;
        }
        catch (InvalidCastException ex) when (ex.Message.IndexOf("is not supported", StringComparison.OrdinalIgnoreCase) > 0)
        {
            // System.InvalidCastException : Reading as 'System.Object' is not supported for fields having DataTypeName 'public.inventory_item'
            value = null;
            return false;
        }
    }
}