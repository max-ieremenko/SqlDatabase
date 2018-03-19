﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts
{
    internal sealed class TextScript : IScript
    {
        public string DisplayName { get; set; }

        public string Sql { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            var valueByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Action<string, string> onVariablesReplace = (name, value) =>
            {
                if (!valueByName.ContainsKey(name))
                {
                    valueByName.Add(name, value);
                }
            };

            var batches = new List<string>();
            foreach (var batch in SplitByGo(Sql))
            {
                if (!string.IsNullOrEmpty(batch))
                {
                    batches.Add(ApplyVariables(batch, variables, onVariablesReplace));
                }
            }

            if (valueByName.Count > 0)
            {
                foreach (var name in valueByName.Keys.OrderBy(i => i))
                {
                    var value = valueByName[name];
                    logger.Info("variable {0} was replaced with {1}".FormatWith(name, value));
                }
            }

            foreach (var batch in batches)
            {
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }
        }

        internal static string ApplyVariables(string sql, IVariables variables, Action<string, string> onReplace = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return sql;
            }

            MatchEvaluator evaluator = match =>
            {
                var name = match.Value.Substring(2);
                name = name.Substring(0, name.Length - 2);

                var value = variables.GetValue(name);
                if (value == null)
                {
                    throw new InvalidOperationException("Variable [{0}] is not defined.".FormatWith(name));
                }

                onReplace?.Invoke(name, value);
                return value;
            };

            return Regex.Replace(sql, "\\{\\{\\w+\\}\\}", evaluator, RegexOptions.Compiled);
        }

        internal static IEnumerable<string> SplitByGo(string sql)
        {
            var batch = new StringBuilder();

            using (var reader = new StringReader(sql))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
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
    }
}
