using System;
using System.Diagnostics;
using System.IO;
using SqlDatabase.Export;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseExportCommand : DatabaseCommandBase
    {
        public ICreateScriptSequence ScriptSequence { get; set; }

        public Func<TextWriter> OpenOutput { get; set; }

        public string DestinationTableName { get; set; }

        internal Func<IDataExporter> ExporterFactory { get; set; } = () => new DataExporter();

        protected override void Greet(string databaseLocation)
        {
            Log.Info("Export data from {0}".FormatWith(databaseLocation));
        }

        protected override void ExecuteCore()
        {
            var sequences = ScriptSequence.BuildSequence();

            using (var output = OpenOutput())
            {
                var readerIndex = 0;

                var exporter = ExporterFactory();
                exporter.Output = new SqlWriter(output);

                foreach (var script in sequences)
                {
                    var timer = Stopwatch.StartNew();
                    Log.Info("export {0} ...".FormatWith(script.DisplayName));

                    using (Log.Indent())
                    {
                        foreach (var reader in Database.ExecuteReader(script))
                        {
                            readerIndex++;

                            exporter.Export(reader, GetExportTableName(DestinationTableName, readerIndex));
                        }
                    }

                    Log.Info("done in {0}".FormatWith(timer.Elapsed));
                }
            }
        }

        private static string GetExportTableName(string name, int index)
        {
            var result = string.IsNullOrWhiteSpace(name) ? "dbo.SqlDatabaseExport" : name;

            return index > 1 ? result + index : result;
        }
    }
}
