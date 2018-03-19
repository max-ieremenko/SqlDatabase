using System;
using System.Data;
using System.Data.Common;

namespace SqlDatabase.Scripts
{
    public partial class AssemblyScriptTest
    {
        private sealed class DbCommandProxy : DbCommand
        {
            private readonly IDbCommand _command;

            public DbCommandProxy(IDbCommand command)
            {
                _command = command;
            }

            public override string CommandText
            {
                get => _command.CommandText;
                set => _command.CommandText = value;
            }

            public override int CommandTimeout
            {
                get => _command.CommandTimeout;
                set => _command.CommandTimeout = value;
            }

            public override CommandType CommandType
            {
                get => _command.CommandType;
                set => _command.CommandType = value;
            }

            public override UpdateRowSource UpdatedRowSource
            {
                get => _command.UpdatedRowSource;
                set => _command.UpdatedRowSource = value;
            }

            public override bool DesignTimeVisible
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            protected override DbConnection DbConnection
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            protected override DbParameterCollection DbParameterCollection => throw new NotSupportedException();

            protected override DbTransaction DbTransaction
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Cancel()
            {
                _command.Cancel();
            }

            public override void Prepare()
            {
                _command.Prepare();
            }

            public override int ExecuteNonQuery()
            {
                return _command.ExecuteNonQuery();
            }

            public override object ExecuteScalar()
            {
                return _command.ExecuteScalar();
            }

            protected override DbParameter CreateDbParameter()
            {
                throw new NotSupportedException();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                throw new NotSupportedException();
            }
        }
    }
}
