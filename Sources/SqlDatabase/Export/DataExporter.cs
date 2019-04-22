using System;
using System.Data;
using System.Linq;

namespace SqlDatabase.Export
{
    internal sealed class DataExporter : IDataExporter
    {
        public SqlWriter Output { get; set; }

        public int MaxInsertBatchSize { get; set; } = 500;

        public ILogger Log { get; set; }

        public void Export(IDataReader source, string tableName)
        {
            ExportTable table;
            using (var metadata = source.GetSchemaTable())
            {
                table = ReadSchemaTable(metadata, tableName);
            }

            CreateTable(table);

            Output.Line();

            var batchNum = 0;
            var rowNum = 0;
            while (source.Read())
            {
                if (rowNum == 0)
                {
                    if (batchNum > 0)
                    {
                        Output.Go().Line();
                    }

                    WriteInsertHeader(table);
                }
                else
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

                if (rowNum == MaxInsertBatchSize)
                {
                    batchNum++;
                    rowNum = 0;

                    Log.Info("{0} rows".FormatWith(batchNum * MaxInsertBatchSize));
                }
            }

            Output.Go();
            if (rowNum > 0)
            {
                Log.Info("{0} rows".FormatWith((batchNum * MaxInsertBatchSize) + rowNum));
            }
        }

        internal ExportTable ReadSchemaTable(DataTable metadata, string tableName)
        {
            var result = new ExportTable { Name = tableName };

            const string GeneratedName = "GeneratedName";
            var generatedIndex = 0;

            var rows = metadata.Rows.Cast<DataRow>().OrderBy(i => (int)i["ColumnOrdinal"]);
            foreach (var row in rows)
            {
                var name = (string)row["ColumnName"];
                if (string.IsNullOrWhiteSpace(name))
                {
                    generatedIndex++;
                    name = GeneratedName + generatedIndex;
                }

                var typeName = (string)row["DataTypeName"];
                var size = (int)row["ColumnSize"];

                if ("timestamp".Equals(typeName, StringComparison.OrdinalIgnoreCase)
                    || "RowVersion".Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    typeName = "VARBINARY";
                    size = 8;
                }
                else if (typeName.EndsWith("sys.HIERARCHYID", StringComparison.OrdinalIgnoreCase))
                {
                    // System.IO.FileNotFoundException : Could not load file or assembly 'Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91' or one of its dependencies
                    throw new NotSupportedException("Data type hierarchyid is not supported, to export data convert value to NVARCHAR: SELECT CAST([{0}] AND NVARCHAR(100)) [{0}]".FormatWith(name));
                }

                result.Columns.Add(new ExportTableColumn
                {
                    Name = name,
                    SqlDataTypeName = typeName,
                    Size = size,
                    NumericPrecision = (short?)DataReaderTools.CleanValue(row["NumericPrecision"]),
                    NumericScale = (short?)DataReaderTools.CleanValue(row["NumericScale"]),
                    AllowNull = (bool)row["AllowDBNull"]
                });
            }

            return result;
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
    }
}
