using System;
using System.Globalization;
using System.IO;

namespace SqlDatabase.Export
{
    internal sealed class SqlWriter : IDisposable
    {
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

            var type = value.GetType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    Output.Write((bool)value ? "1" : "0");
                    break;
                case TypeCode.Char:
                    break;
                case TypeCode.SByte:
                    break;
                case TypeCode.UInt16:
                    break;
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Int16:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    Output.Write(Convert.ToString(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.UInt32:
                    break;
                case TypeCode.UInt64:
                    break;
                case TypeCode.DateTime:
                    break;
                case TypeCode.String:
                    Output.Write("N'{0}'", value);
                    break;
                ////case TypeCode.Empty:
                ////case TypeCode.DBNull:
                ////case TypeCode.Object:
                default:
                    throw new NotSupportedException("Type [{0}] is not supported.".FormatWith(type));
            }

            return this;
        }
    }
}
