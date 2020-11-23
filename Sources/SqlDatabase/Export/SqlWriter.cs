using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SqlDatabase.Export
{
    internal sealed class SqlWriter : IDisposable
    {
        private const char Q = '\'';

        public SqlWriter(TextWriter output)
        {
            Output = output;
        }

        public TextWriter Output { get; }

        public void Dispose()
        {
            Output.Dispose();
        }

        public SqlWriter Line(string value = null)
        {
            Output.WriteLine(value);
            return this;
        }

        public SqlWriter Go()
        {
            Output.WriteLine("GO");
            return this;
        }

        public SqlWriter Text(string value)
        {
            Output.Write(value);
            return this;
        }

        public SqlWriter TextFormat(string format, params object[] args)
        {
            Output.Write(format, args);
            return this;
        }

        public SqlWriter Name(string value)
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

        public SqlWriter DataType(string typeName, int size, int precision, int scale)
        {
            var name = typeName.ToUpperInvariant();
            string sizeText = null;

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
                        sizeText = "{0},{1}".FormatWith(precision, scale);
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

        public SqlWriter Null()
        {
            Output.Write("NULL");
            return this;
        }

        public SqlWriter Value(object value)
        {
            value = DataReaderTools.CleanValue(value);
            if (value == null)
            {
                Null();
                return this;
            }

            if (!TryWriteValue(value))
            {
                throw new NotSupportedException("Type [{0}] is not supported.".FormatWith(value.GetType()));
            }

            return this;
        }

        private bool TryWriteValue(object value)
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

        private void ValueString(string value)
        {
            Output.Write('N');
            Output.Write(Q);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                Output.Write(c);

                if (c == '\'')
                {
                    Output.Write(Q);
                }
            }

            Output.Write(Q);
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
            var buff = new StringBuilder(value.Length * 2);
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
}
