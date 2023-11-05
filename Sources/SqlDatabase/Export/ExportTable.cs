using System.Collections.Generic;

namespace SqlDatabase.Export;

internal sealed class ExportTable
{
    public ExportTable(string name)
    {
        Name = name;
        Columns = new List<ExportTableColumn>();
    }

    public string Name { get; }

    public IList<ExportTableColumn> Columns { get; }
}