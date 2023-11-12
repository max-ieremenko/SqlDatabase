using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlDatabase.Adapter.MySql;

internal sealed class MySqlWriter : SqlWriterBase
{
    public MySqlWriter(TextWriter output)
        : base(output)
    {
    }

    public override SqlWriterBase Name(string value)
    {
        Output.Write(value);
        return this;
    }

    public override SqlWriterBase BatchSeparator()
    {
        Output.WriteLine(";");
        return this;
    }

    public override string GetDefaultTableName() => "sqldatabase_export";

    public override SqlWriterBase DataType(string typeName, int size, int precision, int scale)
    {
        var name = typeName.ToUpperInvariant();
        string? sizeText = null;

        switch (name)
        {
            case "TINYINT":
            case "SMALLINT":
            case "MEDIUMINT":
            case "INT":
            case "BIGINT":
            case "BIT":
            case "NCHAR":
            case "NVARCHAR":
            case "BINARY":
            case "VARBINARY":
                if (size != 0)
                {
                    sizeText = size.ToString(CultureInfo.InvariantCulture);
                }

                break;
            case "TINYINT UNSIGNED":
            case "SMALLINT UNSIGNED":
            case "MEDIUMINT UNSIGNED":
            case "INT UNSIGNED":
            case "BIGINT UNSIGNED":
                if (size != 0)
                {
                    name = name.Insert(name.IndexOf(' '), $"({size})");
                }

                break;

            case "NUMERIC":
                if (scale != 0)
                {
                    sizeText = $"{precision},{scale}";
                }
                else if (precision != 0)
                {
                    sizeText = precision.ToString(CultureInfo.InvariantCulture);
                }

                break;

            case "FLOAT":
            case "DOUBLE":
                break;
        }

        Output.Write(name);
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

            var typeName = GetDataTypeName((int)row["ProviderType"]);
            var size = (int)row["ColumnSize"];

            result.Columns.Add(new ExportTableColumn
            {
                Name = name,
                SqlDataTypeName = typeName,
                Size = size,
                NumericPrecision = (int?)DataReaderTools.CleanValue(row["NumericPrecision"]),
                NumericScale = (int?)DataReaderTools.CleanValue(row["NumericScale"]),
                AllowNull = (bool?)DataReaderTools.CleanValue(row["AllowDBNull"]) ?? true
            });
        }

        return result;
    }

    protected override bool TryWriteValue(object value, string? typeNameHint)
    {
        var type = value.GetType();

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                Output.Write((bool)value ? "TRUE" : "FALSE");
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
                Output.Write(((double)value).ToString("G17", CultureInfo.InvariantCulture));
                return true;

            case TypeCode.DateTime:
                ValueDateTime((DateTime)value);
                return true;

            case TypeCode.String:
                if (typeNameHint == "GEOMETRY")
                {
                    Output.Write((string)value);
                }
                else
                {
                    Output.Write('N');
                    ValueString((string)value);
                }

                return true;
        }

        if (value is TimeSpan timeSpan)
        {
            ValueDateTime(new DateTime(1970, 01, 01).Add(timeSpan));
            return true;
        }

        if (value is byte[] array)
        {
            ValueByteArray(array);
            return true;
        }

        return false;
    }

    private static string GetDataTypeName(int providerType)
    {
        // MySqlDbType
        switch (providerType)
        {
            case -1:
                return "BOOL";
            case 1:
                return "TINYINT";
            case 501:
                return "TINYINT UNSIGNED";
            case 2:
                return "SMALLINT";
            case 502:
                return "SMALLINT UNSIGNED";
            case 3:
                return "INT";
            case 503:
                return "INT UNSIGNED";
            case 4:
                return "FLOAT";
            case 5:
                return "DOUBLE";
            case 7:
                return "TIMESTAMP";
            case 8:
                return "BIGINT";
            case 508:
                return "BIGINT UNSIGNED";
            case 9:
                return "MEDIUMINT";
            case 509:
                return "MEDIUMINT UNSIGNED";
            case 10:
                return "DATE";
            case 11:
                return "TIME";
            case 12:
                return "DATETIME";
            case 13:
                return "YEAR";
            case 16:
                return "BIT";
            case 245:
                return "JSON";
            case 246:
                return "NUMERIC";
            case 247:
                // ENUM
                return "NVARCHAR";
            case 248:
                // SET
                return "NVARCHAR";
            case 252:
                return "BLOB";
            case 253:
                return "NVARCHAR";
            case 254:
                return "NCHAR";
            case 255:
                return "GEOMETRY";
            case 600:
                return "BINARY";
            case 601:
                return "VARBINARY";
            case 752:
                return "TEXT";
        }

        return "NVARCHAR";
    }

    private void ValueDateTime(DateTime value)
    {
        Output.Write(Q);
        Output.Write(value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
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
}