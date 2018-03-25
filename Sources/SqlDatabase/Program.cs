using System;
using System.Diagnostics;
using System.IO;
using SqlDatabase.Configuration;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase
{
    internal static class Program
    {
        private static ILogger _logger;

        public static int Main(string[] args)
        {
            var exitCode = ExitCode.Ok;
            _logger = new ConsoleLogger();

            var cmd = ParseCommandLine(args);
            if (cmd == null)
            {
                _logger.Info(LoadHelpContent());
                exitCode = ExitCode.InvalidCommandLine;
            }
            else if (cmd.Command == Command.Upgrade)
            {
                exitCode = DoUpgrade(cmd) ? ExitCode.Ok : ExitCode.ExecutionErrors;
            }
            else if (cmd.Command == Command.Create)
            {
                exitCode = DoCreate(cmd) ? ExitCode.Ok : ExitCode.ExecutionErrors;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            return (int)exitCode;
        }

        private static bool DoCreate(CommandLine cmd)
        {
            _logger.Info("Create database [{0}] on [{1}]".FormatWith(
                cmd.Connection.InitialCatalog,
                cmd.Connection.DataSource));

            var database = new Database
            {
                ConnectionString = cmd.Connection.ToString(),
                Log = _logger,
                Configuration = AppConfiguration.GetCurrent(),
            };

            foreach (var entry in cmd.Variables)
            {
                database.Variables.SetValue(entry.Key, entry.Value);
            }

            var create = new SequentialCreate
            {
                Log = _logger,
                Database = database,
                ScriptSequence = new CreateScriptSequence
                {
                    Root = FileSytemFactory.FolderFromPath(cmd.Scripts),
                    ScriptFactory = new ScriptFactory()
                }
            };

            try
            {
                create.Execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Info(ex.ToString());

                return false;
            }

            return true;
        }

        private static bool DoUpgrade(CommandLine cmd)
        {
            _logger.Info("Upgrade database [{0}] on [{1}]".FormatWith(
                             cmd.Connection.InitialCatalog,
                             cmd.Connection.DataSource));

            var database = new Database
            {
                ConnectionString = cmd.Connection.ToString(),
                Log = _logger,
                Configuration = AppConfiguration.GetCurrent(),
                Transaction = cmd.Transaction
            };

            foreach (var entry in cmd.Variables)
            {
                database.Variables.SetValue(entry.Key, entry.Value);
            }

            var upgrade = new SequentialUpgrade
            {
                Log = _logger,
                Database = database,
                ScriptSequence = new UpgradeScriptSequence
                {
                    Root = FileSytemFactory.FolderFromPath(cmd.Scripts),
                    ScriptFactory = new ScriptFactory()
                }
            };

            try
            {
                upgrade.Execute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Info(ex.ToString());

                return false;
            }

            return true;
        }

        private static CommandLine ParseCommandLine(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            try
            {
                return CommandLine.Parse(args);
            }
            catch (Exception e)
            {
                _logger.Error("Invalid command line: {0}".FormatWith(e.Message));
            }

            return null;
        }

        private static string LoadHelpContent()
        {
            var scope = typeof(CommandLine);
            using (var stream = scope.Assembly.GetManifestResourceStream(scope, "CommandLine.txt"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}