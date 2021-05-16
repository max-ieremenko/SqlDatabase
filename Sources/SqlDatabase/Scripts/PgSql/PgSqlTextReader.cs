using System.Collections.Generic;
using System.IO;

namespace SqlDatabase.Scripts.PgSql
{
    internal sealed class PgSqlTextReader : ISqlTextReader
    {
        public IEnumerable<string> Read(Stream sql)
        {
            string script;
            using (var reader = new StreamReader(sql))
            {
                script = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(script))
            {
                return new string[0];
            }

            return new[] { script };
        }
    }
}
