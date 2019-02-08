using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgram : ISqlDatabaseProgram
    {
        private readonly PSCmdlet _owner;
        private readonly ILogger _logger;
        private readonly OutputReader _reader;

        public SqlDatabaseProgram(PSCmdlet owner)
        {
            _owner = owner;
            _logger = new CmdLetLogger(owner);
            _reader = new OutputReader();
        }

        public void ExecuteCommand(CommandLine command)
        {
            int exitCode;
            using (var process = CreateProcess(command))
            {
                process.OutputDataReceived += ProcessOutputDataReceived;

                process.Start();
                process.BeginOutputReadLine();

                process.WaitForExit();
                process.OutputDataReceived -= ProcessOutputDataReceived;

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
            {
                _owner.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("Execution failed."), null, ErrorCategory.NotSpecified, null));
            }
        }

        private static Process CreateProcess(CommandLine command)
        {
            var sqlDatabase = typeof(Program).Assembly.Location;

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(sqlDatabase),
                FileName = Path.GetFileName(sqlDatabase),
                UseShellExecute = false,
                Arguments = string.Join(" ", new CommandLineBuilder(command).BuildArray().Select(i => "\"" + i + "\"")),
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            return new Process
            {
                StartInfo = startInfo,
                SynchronizingObject = new Synchronizer()
            };
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                var line = _reader.NextLine(e.Data);

                if (line.Value)
                {
                    _logger.Error(line.Key);
                }
                else
                {
                    _logger.Info(line.Key);
                }
            }
        }

        private sealed class Synchronizer : ISynchronizeInvoke
        {
            private readonly object _syncRoot = new object();

            public bool InvokeRequired => true;

            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                throw new NotSupportedException();
            }

            public object EndInvoke(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            public object Invoke(Delegate method, object[] args)
            {
                lock (_syncRoot)
                {
                    return method.DynamicInvoke(args);
                }
            }
        }
    }
}
