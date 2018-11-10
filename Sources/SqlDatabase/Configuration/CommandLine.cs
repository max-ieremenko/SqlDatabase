using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlDatabase.Configuration
{
    public sealed class CommandLine
    {
        public Command Command { get; set; }

        public SqlConnectionStringBuilder Connection { get; set; }

        public TransactionMode Transaction { get; set; }

        public IList<string> Scripts { get; } = new List<string>();

        public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string ConfigurationFile { get; set; }
    }
}