namespace SqlDatabase.Adapter.PgSql.UnmappedTypes;

internal sealed class Composite
{
    public Composite(string[] names)
    {
        Names = names;
        Rows = new(0);
    }

    public string[] Names { get; }

    public List<object?[]> Rows { get; }
}