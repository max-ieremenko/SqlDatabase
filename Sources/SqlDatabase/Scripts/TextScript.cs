using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlDatabase.Scripts
{
    internal sealed class TextScript : IScript
    {
        public string DisplayName { get; set; }

        public Func<Stream> ReadSqlContent { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            var batches = ResolveBatches(variables, logger);

            foreach (var batch in batches)
            {
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger)
        {
            var batches = ResolveBatches(variables, logger);

            foreach (var batch in batches)
            {
                command.CommandText = batch;
                using (var reader = command.ExecuteReader())
                {
                    yield return reader;
                }
            }
        }

        public IList<ScriptDependency> GetDependencies()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> ResolveBatches(IVariables variables, ILogger logger)
        {
            var scriptParser = new SqlScriptVariableParser(variables);

            var batches = new List<string>();
            using (var sql = ReadSqlContent())
            {
                foreach (var batch in SqlBatchParser.SplitByGo(sql))
                {
                    var script = scriptParser.ApplyVariables(batch);
                    if (!string.IsNullOrEmpty(script))
                    {
                        batches.Add(script);
                    }
                }
            }

            var report = scriptParser.ValueByName.OrderBy(i => i.Key);
            foreach (var entry in report)
            {
                logger.Info("variable {0} was replaced with {1}".FormatWith(entry.Key, entry.Value));
            }

            return batches;
        }
    }
}
