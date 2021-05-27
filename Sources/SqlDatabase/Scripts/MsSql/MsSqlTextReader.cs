using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts.MsSql
{
    internal sealed class MsSqlTextReader : ISqlTextReader
    {
        private readonly Regex _goRegex = new Regex("^(\\s*(go)+\\s*)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string ReadFirstBatch(Stream sql)
        {
            return ReadBatches(sql).FirstOrDefault();
        }

        public IEnumerable<string> ReadBatches(Stream sql)
        {
            var batch = new StringBuilder();

            foreach (var line in ReadLines(sql))
            {
                if (IsGo(line))
                {
                    if (batch.Length > 0)
                    {
                        yield return batch.ToString();
                        batch.Clear();
                    }
                }
                else if (batch.Length > 0 || line.Trim().Length > 0)
                {
                    if (batch.Length > 0)
                    {
                        batch.AppendLine();
                    }

                    batch.Append(line);
                }
            }

            if (batch.Length > 0)
            {
                yield return batch.ToString();
            }
        }

        internal bool IsGo(string text) => _goRegex.IsMatch(text);

        private static IEnumerable<string> ReadLines(Stream sql)
        {
            using (var reader = new StreamReader(sql))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
