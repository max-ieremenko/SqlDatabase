using System.Globalization;

namespace SqlDatabase.Adapter.MsSql;

internal sealed class MsSqlWriter : SqlWriterBase
{
    public MsSqlWriter(TextWriter output)
        : base(output)
    {
    }

    public override SqlWriterBase Name(string value)
    {
        const char OpenBracket = '[';
        const char CloseBracket = ']';

        if (value[0] != OpenBracket)
        {
            Output.Write(OpenBracket);
        }

        Output.Write(value);

        if (value[value.Length - 1] != CloseBracket)
        {
            Output.Write(CloseBracket);
        }

        return this;
    }

    public override SqlWriterBase BatchSeparator()
    {
        Output.WriteLine("GO");
        return this;
    }

    public override SqlWriterBase DataType(string typeName, int size, int precision, int scale)
    {
        var name = typeName.ToUpperInvariant();
        string? sizeText = null;

        switch (name)
        {
            case "INT":
            case "TINYINT":
            case "SMALLINT":
            case "BIGINT":
                break;
            case "DECIMAL":
                if (scale != 0)
                {
                    sizeText = $"{precision},{scale}";
                }
                else if (precision != 18)
                {
                    // 18 is default
                    sizeText = precision.ToString(CultureInfo.InvariantCulture);
                }

                break;
            case "REAL":
                name = "FLOAT";

                break;
            case "CHAR":
            case "NCHAR":
            case "VARCHAR":
            case "NVARCHAR":
            case "BINARY":
            case "VARBINARY":
                if (size <= 0 || size == int.MaxValue)
                {
                    sizeText = "MAX";
                }
                else if (size != 1)
                {
                    sizeText = size.ToString(CultureInfo.InvariantCulture);
                }

                break;
        }

        Output.Write(name.ToUpperInvariant());
        if (sizeText != null)
        {
            Output.Write("(");
            Output.Write(sizeText);
            Output.Write(")");
        }

        return this;
    }

    public override ExportTable ReadSchemaTable(DataTable metadata, string tableName)
    {
        var result = new ExportTable(tableName);

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
                throw new NotSupportedException($"Data type hierarchyid is not supported, to export data convert value to NVARCHAR: SELECT CAST([{name}] AND NVARCHAR(100)) [{name}]");
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

    public override string GetDefaultTableName() => "dbo.SqlDatabaseExport";

    protected override bool TryWriteValue(object value, string? typeNameHint)
    {
        var type = value.GetType();

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                Output.Write((bool)value ? "1" : "0");
                return true;

            case TypeCode.Char:
                break;
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.Byte:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Int16:
            case TypeCode.Single:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Decimal:
                Output.Write(Convert.ToString(value, CultureInfo.InvariantCulture));
                return true;

            case TypeCode.Double:
                ValueDouble((double)value);
                return true;

            case TypeCode.DateTime:
                ValueDate((DateTime)value);
                return true;

            case TypeCode.String:
                Output.Write('N');
                ValueString((string)value);
                return true;
        }

        if (value is Guid id)
        {
            ValueGuid(id);
            return true;
        }

        if (value is byte[] array)
        {
            ValueByteArray(array);
            return true;
        }

        if (value is TimeSpan timeSpan)
        {
            ValueTimeSpan(timeSpan);
            return true;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            ValueDateTimeOffset(dateTimeOffset);
            return true;
        }

        return false;
    }

    private void ValueDate(DateTime value)
    {
        Output.Write(Q);
        Output.Write(value.ToString("yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture));
        Output.Write(Q);
    }

    private void ValueGuid(Guid value)
    {
        Output.Write(Q);
        Output.Write(value);
        Output.Write(Q);
    }

    private void ValueByteArray(byte[] value)
    {
        var buff = new StringBuilder((value.Length * 2) + 2);
        buff.Append("0x");
        for (var i = 0; i < value.Length; i++)
        {
            buff.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", value[i]);
        }

        Output.Write(buff.ToString());
    }

    private void ValueDouble(double value)
    {
        Output.Write(value.ToString("G17", CultureInfo.InvariantCulture));
    }

    private void ValueTimeSpan(TimeSpan value)
    {
        Output.Write(Q);
        Output.Write(value.ToString("g", CultureInfo.InvariantCulture));
        Output.Write(Q);
    }

    private void ValueDateTimeOffset(DateTimeOffset value)
    {
        Output.Write(Q);
        Output.Write(value.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture));
        Output.Write(Q);
    }
}