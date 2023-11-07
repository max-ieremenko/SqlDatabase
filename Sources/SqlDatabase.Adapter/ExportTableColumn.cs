namespace SqlDatabase.Adapter;

public struct ExportTableColumn
{
    public string Name { get; set; }

    public string SqlDataTypeName { get; set; }

    public int Size { get; set; }

    public int? NumericPrecision { get; set; }

    public int? NumericScale { get; set; }

    public bool AllowNull { get; set; }

    public override string ToString()
    {
        return $"{Name} {SqlDataTypeName}({Size})";
    }
}