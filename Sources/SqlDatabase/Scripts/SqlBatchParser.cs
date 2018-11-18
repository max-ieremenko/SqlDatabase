using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts
{
    internal static class SqlBatchParser
    {
        public static IEnumerable<string> SplitByGo(Stream sql)
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

        internal static bool IsGo(string text)
        {
            return Regex.IsMatch(text, "^(\\s*(go)+\\s*)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

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
