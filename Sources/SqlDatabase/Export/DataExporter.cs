using System.Data;
using System.Linq;

namespace SqlDatabase.Export
{
    internal sealed class DataExporter : IDataExporter
    {
        public SqlWriter Output { get; set; }

        public void Export(IDataReader source, string tableName)
        {
            ExportTable table;
            using (var metadata = source.GetSchemaTable())
            {
                table = ReadSchemaTable(metadata, tableName);
            }

            CreateTable(table);

            Output.Line();

            WriteInsertHeader(table);

            var rowNum = 0;
            while (source.Read())
            {
                if (rowNum > 0)
                {
                    Output.Text("      ,");
                }

                Output.Text("(");

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        Output.Text(", ");
                    }

                    Output.Value(source[i]);
                }

                Output.Line(")");
                rowNum++;
            }

            Output.Go();
        }

        private void CreateTable(ExportTable table)
        {
            Output
                .Text("CREATE TABLE ")
                .Text(table.Name)
                .Line()
                .Line("(");

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                Output.Text("    ");
                if (i > 0)
                {
                    Output.Text(",");
                }

                Output
                    .Name(column.Name)
                    .Text(" ")
                    .DataType(column.SqlDataTypeName, column.Size, column.NumericPrecision ?? 0, column.NumericScale ?? 0)
                    .Text(" ");

                if (!column.AllowNull)
                {
                    Output.Text("NOT ");
                }

                Output
                    .Null()
                    .Line();
            }

            Output
                .Line(")")
                .Go();
        }

        private void WriteInsertHeader(ExportTable table)
        {
            Output
                .Text("INSERT INTO ")
                .Text(table.Name)
                .Text("(");

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                if (i > 0)
                {
                    Output.Text(", ");
                }

                Output.Name(column.Name);
            }

            Output
                .Line(")")
                .Text("VALUES ");
        }

        private ExportTable ReadSchemaTable(DataTable metadata, string tableName)
        {
            var result = new ExportTable { Name = tableName };

            var rows = metadata.Rows.Cast<DataRow>().OrderBy(i => (int)i["ColumnOrdinal"]);

            foreach (var row in rows)
            {
                result.Columns.Add(new ExportTableColumn
                {
                    Name = (string)row["ColumnName"],
                    SqlDataTypeName = (string)row["DataTypeName"],
                    Size = (int)row["ColumnSize"],
                    NumericPrecision = (short?)DataReaderTools.CleanValue(row["NumericPrecision"]),
                    NumericScale = (short?)DataReaderTools.CleanValue(row["NumericScale"]),
                    AllowNull = (bool)row["AllowDBNull"]
                });
            }

            return result;
        }
    }
}
