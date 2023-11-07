using System.Collections.Generic;

namespace SqlDatabase.Adapter;

public sealed class ExportTable
{
    public ExportTable(string name)
    {
        Name = name;
        Columns = new List<ExportTableColumn>();
    }

    public string Name { get; }

    public List<ExportTableColumn> Columns { get; }
}