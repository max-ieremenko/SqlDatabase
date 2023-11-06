using System.Data;
using SqlDatabase.Adapter;

namespace SqlDatabase.Export;

internal sealed class DataExporter : IDataExporter
{
    public SqlWriterBase Output { get; set; } = null!;

    public int MaxInsertBatchSize { get; set; } = 500;

    public ILogger Log { get; set; } = null!;

    public void Export(IDataReader source, string tableName)
    {
        ExportTable table;
        using (var metadata = source.GetSchemaTable())
        {
            table = Output.ReadSchemaTable(metadata!, tableName);
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
                    Output.BatchSeparator().Line();
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

                Output.Value(source[i], table.Columns[i].SqlDataTypeName);
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

        Output.BatchSeparator();
        if (rowNum > 0)
        {
            Log.Info("{0} rows".FormatWith((batchNum * MaxInsertBatchSize) + rowNum));
        }
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
            .BatchSeparator();
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