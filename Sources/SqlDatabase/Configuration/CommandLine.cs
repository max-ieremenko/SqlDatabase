using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlDatabase.Configuration
{
    internal sealed class CommandLine
    {
        public Command Command { get; set; }

        public SqlConnectionStringBuilder Connection { get; set; }

        public TransactionMode Transaction { get; set; }

        public string Scripts { get; set; }

        public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}