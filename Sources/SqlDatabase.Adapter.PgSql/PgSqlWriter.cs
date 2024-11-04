using System.Collections;
using System.Globalization;
using System.Reflection;
using NpgsqlTypes;
using SqlDatabase.Adapter.PgSql.UnmappedTypes;

namespace SqlDatabase.Adapter.PgSql;

internal sealed class PgSqlWriter : SqlWriterBase
{
    public PgSqlWriter(TextWriter output)
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

    public override SqlWriterBase DataType(string typeName, int size, int precision, int scale)
    {
        var name = typeName.ToUpperInvariant();
        string? sizeText = null;

        switch (name)
        {
            case "INTEGER":
            case "SMALLINT":
            case "BIGINT":
                break;
            case "NUMERIC":
            case "TIMESTAMP":
            case "INTERVAL":
            case "TIME":
                if (scale != 0)
                {
                    sizeText = $"{precision},{scale}";
                }
                else if (precision != 0)
                {
                    sizeText = precision.ToString(CultureInfo.InvariantCulture);
                }

                break;
            case "CHARACTER VARYING":
            case "CHARACTER":
            case "BIT":
                if (size > 0)
                {
                    sizeText = size.ToString(CultureInfo.InvariantCulture);
                }

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

            var typeName = (string)row["DataTypeName"];
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

    public override string GetDefaultTableName() => "public.sqldatabase_export";

    protected override bool TryWriteValue(object value, string? typeNameHint)
    {
        var type = value.GetType();

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                if (string.Equals(typeNameHint, "bit", StringComparison.OrdinalIgnoreCase))
                {
                    Output.Write('B');
                    Output.Write(Q);
                    Output.Write((bool)value ? "1" : "0");
                    Output.Write(Q);
                }
                else
                {
                    Output.Write((bool)value ? "true" : "false");
                }

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
                if (typeNameHint?.IndexOf('[') > 0)
                {
                    // '{{"meeting", "lunch"}, {"meeting"}}'
                    ValueString((string)value, '"');
                }
                else
                {
                    ValueString((string)value);
                }

                return true;
        }

        if (value is Guid id)
        {
            ValueGuid(id);
            return true;
        }

        if (value is byte[] byteArray)
        {
            ValueByteArray(byteArray);
            return true;
        }

        if (value is TimeSpan timeSpan)
        {
            ValueTimeSpan(timeSpan);
            return true;
        }

        if (value is BitArray bitArray)
        {
            ValueBitArray(bitArray);
            return true;
        }

        if (value is NpgsqlTsVector vector)
        {
            ValueTsVector(vector);
            return true;
        }

        if (value is NpgsqlTsQuery query)
        {
            ValueTsQuery(query);
            return true;
        }

        if (value is Array array)
        {
            if (array.Rank == 1)
            {
                Value1dArray(array, typeNameHint);
                return true;
            }

            if (array.Rank == 2)
            {
                Value2dArray(array, typeNameHint);
                return true;
            }

            throw new NotSupportedException($"{array.Rank}d array is not supported.");
        }

        if (value is Composite composite)
        {
            ValueComposite(composite);
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NpgsqlRange<>))
        {
            GetType()
                .GetMethod(nameof(ValueRange), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!
                .MakeGenericMethod(type.GenericTypeArguments)
                .Invoke(this, new[] { value });
            return true;
        }

        return false;
    }

    private void ValueDate(DateTime value)
    {
        Output.Write(Q);
        Output.Write(value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
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
        var buff = new StringBuilder((value.Length * 2) + 4);
        buff.Append(Q).Append("\\x");
        for (var i = 0; i < value.Length; i++)
        {
            buff.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", value[i]);
        }

        buff.Append(Q);
        Output.Write(buff.ToString());
    }

    private void ValueBitArray(BitArray value)
    {
        var buff = new StringBuilder((value.Length * 2) + 4);
        buff.Append('B').Append(Q);
        for (var i = 0; i < value.Length; i++)
        {
            buff.Append(value[i] ? '1' : '0');
        }

        buff.Append(Q);
        Output.Write(buff.ToString());
    }

    private void ValueDouble(double value)
    {
        Output.Write(value.ToString("G17", CultureInfo.InvariantCulture));
    }

    private void ValueTimeSpan(TimeSpan value)
    {
        Output.Write(Q);

        if (value.Days > 0)
        {
            Output.Write(value.Days.ToString(CultureInfo.InvariantCulture));
            Output.Write(" ");
            value = value.Add(-TimeSpan.FromDays(value.Days));
        }

        Output.Write(value.ToString("g", CultureInfo.InvariantCulture));
        Output.Write(Q);
    }

    private void ValueTsVector(NpgsqlTsVector value)
    {
        Output.Write(Q);
        for (var i = 0; i < value.Count; i++)
        {
            if (i > 0)
            {
                Output.Write(' ');
            }

            var lexeme = value[i];
            WriteEscapedString(lexeme.Text);

            if (lexeme.Count > 0)
            {
                Output.Write(':');
                for (var j = 0; j < lexeme.Count; j++)
                {
                    if (j > 0)
                    {
                        Output.Write(',');
                    }

                    Output.Write(lexeme[j].ToString());
                }
            }
        }

        Output.Write(Q);
    }

    private void ValueTsQuery(NpgsqlTsQuery value)
    {
        var valueByLexeme = new Dictionary<NpgsqlTsQueryLexeme, (string Original, string Name)>();
        CollectLexeme(value, valueByLexeme);

        var text = new StringBuilder(value.ToString());
        foreach (var entry in valueByLexeme.Values)
        {
            text.Replace(entry.Name, entry.Original);
        }

        Output.Write(Q);
        WriteEscapedString(text.ToString());
        Output.Write(Q);
    }

    private void CollectLexeme(NpgsqlTsQuery value, IDictionary<NpgsqlTsQueryLexeme, (string Original, string Name)> valueByLexeme)
    {
        if (value is NpgsqlTsQueryLexeme lexeme)
        {
            if (!valueByLexeme.ContainsKey(lexeme))
            {
                var name = "___sqldb___" + valueByLexeme.Count.ToString(CultureInfo.InvariantCulture);
                valueByLexeme.Add(lexeme, (lexeme.Text, Q + name + Q));
                lexeme.Text = name;
            }

            return;
        }

        if (value is NpgsqlTsQueryNot not)
        {
            if (not.Child != null)
            {
                CollectLexeme(not.Child, valueByLexeme);
            }

            return;
        }

        if (value is NpgsqlTsQueryBinOp bin)
        {
            if (bin.Left != null)
            {
                CollectLexeme(bin.Left, valueByLexeme);
            }

            if (bin.Right != null)
            {
                CollectLexeme(bin.Right, valueByLexeme);
            }
        }
    }

    private void Value1dArray(Array value, string? typeNameHint)
    {
        Output.Write(Q);
        Output.Write('{');
        for (var i = 0; i < value.Length; i++)
        {
            if (i > 0)
            {
                Output.Write(", ");
            }

            Value(value.GetValue(i), typeNameHint);
        }

        Output.Write('}');
        Output.Write(Q);
    }

    private void Value2dArray(Array value, string? typeNameHint)
    {
        Output.Write(Q);
        Output.Write('{');

        if (value.Length > 0)
        {
            var length1 = value.GetLength(0);
            var length2 = value.GetLength(1);

            for (var i1 = 0; i1 < length1; i1++)
            {
                if (i1 > 0)
                {
                    Output.Write(", ");
                }

                Output.Write('{');
                for (var i2 = 0; i2 < length2; i2++)
                {
                    if (i2 > 0)
                    {
                        Output.Write(", ");
                    }

                    Value(value.GetValue(i1, i2), typeNameHint);
                }

                Output.Write('}');
            }
        }

        Output.Write('}');
        Output.Write(Q);
    }

    private void ValueComposite(Composite value)
    {
        if (value.Rows.Count != 1)
        {
            throw new NotSupportedException($"Composite object with {value.Rows.Count} rows is not supported.");
        }

        Output.Write("ROW(");

        var row = value.Rows[0];
        for (var i = 0; i < row.Length; i++)
        {
            if (i > 0)
            {
                Output.Write(", ");
            }

            Value(row[i]);
        }

        Output.Write(')');
    }

    private void ValueRange<T>(NpgsqlRange<T> value)
    {
        Output.Write(Q);

        if (value.IsEmpty)
        {
            Output.Write("empty");
        }
        else
        {
            Output.Write(value.LowerBoundIsInclusive ? '[' : '(');
            if (!value.LowerBoundInfinite)
            {
                Value(value.LowerBound);
            }

            Output.Write(',');
            if (!value.UpperBoundInfinite)
            {
                Value(value.UpperBound);
            }

            Output.Write(value.UpperBoundIsInclusive ? ']' : ')');
        }

        Output.Write(Q);
    }
}