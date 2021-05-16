using System;
using System.Data;
using System.IO;

namespace SqlDatabase.Export
{
    internal abstract class SqlWriterBase : IDisposable
    {
        public const char Q = '\'';

        protected SqlWriterBase(TextWriter output)
        {
            Output = output;
        }

        public TextWriter Output { get; }

        public void Dispose()
        {
            Output.Dispose();
        }

        public SqlWriterBase Line(string value = null)
        {
            Output.WriteLine(value);
            return this;
        }

        public SqlWriterBase Text(string value)
        {
            Output.Write(value);
            return this;
        }

        public SqlWriterBase TextFormat(string format, params object[] args)
        {
            Output.Write(format, args);
            return this;
        }

        public abstract SqlWriterBase Name(string value);

        public abstract SqlWriterBase BatchSeparator();

        public abstract SqlWriterBase DataType(string typeName, int size, int precision, int scale);

        public SqlWriterBase Null()
        {
            Output.Write("NULL");
            return this;
        }

        public SqlWriterBase Value(object value, string typeNameHint = null)
        {
            value = DataReaderTools.CleanValue(value);
            if (value == null)
            {
                Null();
                return this;
            }

            if (!TryWriteValue(value, typeNameHint))
            {
                throw new NotSupportedException("Type [{0}] is not supported.".FormatWith(value.GetType()));
            }

            return this;
        }

        public abstract ExportTable ReadSchemaTable(DataTable metadata, string tableName);

        public abstract string GetDefaultTableName();

        protected abstract bool TryWriteValue(object value, string typeNameHint);

        protected void ValueString(string value, char q = Q)
        {
            Output.Write(q);
            WriteEscapedString(value);
            Output.Write(q);
        }

        protected void WriteEscapedString(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                Output.Write(c);

                if (c == '\'')
                {
                    Output.Write(Q);
                }
            }
        }
    }
}
