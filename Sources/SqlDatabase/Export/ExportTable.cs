using System.Collections.Generic;

namespace SqlDatabase.Export
{
    internal sealed class ExportTable
    {
        public string[] Name { get; set; }

        public IList<ExportTableColumn> Columns { get; } = new List<ExportTableColumn>();
    }
}
