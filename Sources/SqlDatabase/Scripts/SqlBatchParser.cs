using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TSQL;
using TSQL.Tokens;

namespace SqlDatabase.Scripts
{
    internal static class SqlBatchParser
    {
        public static IEnumerable<string> SplitByGo(Stream sql)
        {
            var script = Read(sql);

            var lastIndex = 0;
            using (var reader = new TSQLStatementReader(script))
            {
                var tokens = reader.SelectMany(i => i.Tokens).Where(IsGo);
                foreach (var token in tokens)
                {
                    var batch = Trim(script.Substring(lastIndex, token.BeginPosition - lastIndex));

                    if (!string.IsNullOrEmpty(batch))
                    {
                        yield return batch;
                    }

                    lastIndex = token.EndPosition + 1;
                }
            }

            if (lastIndex < script.Length - 1)
            {
                var batch = Trim(script.Substring(lastIndex));

                if (!string.IsNullOrEmpty(batch))
                {
                    yield return batch;
                }
            }
        }

        private static bool IsGo(TSQLToken token)
        {
            return token.Type == TSQLTokenType.Keyword
                   && "GO".Equals(token.Text, StringComparison.OrdinalIgnoreCase);
        }

        private static string Trim(string sql)
        {
            return sql.Trim('\r', '\n', ' ');
        }

        private static string Read(Stream sql)
        {
            using (var reader = new StreamReader(sql))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
