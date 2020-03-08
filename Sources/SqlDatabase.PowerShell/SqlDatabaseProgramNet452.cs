using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgramNet452 : ISqlDatabaseProgram
    {
        private readonly ICmdlet _owner;
        private readonly ILogger _logger;
        private readonly OutputReader _reader;

        public SqlDatabaseProgramNet452(ICmdlet owner)
        {
            _owner = owner;
            _logger = new CmdLetLogger(owner);
            _reader = new OutputReader();
        }

        public void ExecuteCommand(GenericCommandLine command)
        {
            command.PreFormatOutputLogs = true;

            bool hasErrors;
            using (var process = CreateProcess(command))
            {
                process.Start();

                hasErrors = WaitForExit(process);
            }

            if (hasErrors)
            {
                _owner.WriteErrorLine("Execution failed.");
            }
        }

        private static Process CreateProcess(GenericCommandLine command)
        {
            var assemblyLocation = typeof(Program).Assembly.Location;

            var sqlDatabase = Path.Combine(Path.GetDirectoryName(assemblyLocation), "net452", Path.GetFileNameWithoutExtension(assemblyLocation) + ".exe");
            if (!File.Exists(sqlDatabase))
            {
                // debug
                sqlDatabase = Path.Combine(Path.GetDirectoryName(assemblyLocation), Path.GetFileNameWithoutExtension(assemblyLocation) + ".exe");
            }

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(sqlDatabase),
                FileName = sqlDatabase,
                UseShellExecute = false,
                Arguments = string.Join(" ", new GenericCommandLineBuilder(command).BuildArray(true).Select(i => "\"" + i + "\"")),
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            return new Process { StartInfo = startInfo };
        }

        private bool WaitForExit(Process process)
        {
            var hasErrors = false;

            string line;
            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                var record = _reader.NextLine(line);
                if (record.HasValue)
                {
                    if (record.Value.IsError)
                    {
                        hasErrors = true;
                        _logger.Error(record.Value.Text);
                    }
                    else
                    {
                        _logger.Info(record.Value.Text);
                    }
                }
            }

            process.WaitForExit();

            var buffered = _reader.Flush();
            if (buffered.HasValue)
            {
                if (buffered.Value.IsError)
                {
                    hasErrors = true;
                    _logger.Error(buffered.Value.Text);
                }
                else
                {
                    _logger.Info(buffered.Value.Text);
                }
            }

            return process.ExitCode != 0 && !hasErrors;
        }
    }
}
