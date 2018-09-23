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
        private static readonly ILogger Logger = new ConsoleLogger();

        public static int Main(string[] args)
        {
            ExitCode exitCode;
            var cmd = ParseCommandLine(args);
            if (cmd == null)
            {
                Logger.Info(LoadHelpContent());
                exitCode = ExitCode.InvalidCommandLine;
            }
            else
            {
                exitCode = ExecuteCommand(cmd) ? ExitCode.Ok : ExitCode.ExecutionErrors;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            return (int)exitCode;
        }

        internal static bool ExecuteCommand(CommandLine cmd)
        {
            if (cmd.Command == Command.Upgrade)
            {
                return DoUpgrade(cmd);
            }

            if (cmd.Command == Command.Create)
            {
                return DoCreate(cmd);
            }

            if (cmd.Command == Command.Execute)
            {
                return DoExecute(cmd);
            }

            throw new NotImplementedException();
        }

        private static bool DoCreate(CommandLine cmd)
        {
            Logger.Info("Create database [{0}] on [{1}]".FormatWith(
                cmd.Connection.InitialCatalog,
                cmd.Connection.DataSource));

            var create = new SequentialCreate
            {
                Log = Logger,
                Database = CreateDatabase(cmd),
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
                Logger.Error(ex.Message);
                Logger.Info(ex.ToString());

                return false;
            }

            return true;
        }

        private static bool DoUpgrade(CommandLine cmd)
        {
            Logger.Info("Upgrade database [{0}] on [{1}]".FormatWith(
                             cmd.Connection.InitialCatalog,
                             cmd.Connection.DataSource));

            var upgrade = new SequentialUpgrade
            {
                Log = Logger,
                Database = CreateDatabase(cmd),
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
                Logger.Error(ex.Message);
                Logger.Info(ex.ToString());

                return false;
            }

            return true;
        }

        private static bool DoExecute(CommandLine cmd)
        {
            var file = FileSytemFactory.FileFromPath(cmd.Scripts);
            var script = new ScriptFactory().FromFile(file);

            Logger.Info("Execute script [{0}] on database [{1}] on [{2}]".FormatWith(
                script.DisplayName,
                cmd.Connection.InitialCatalog,
                cmd.Connection.DataSource));

            var database = CreateDatabase(cmd);

            try
            {
                database.BeforeCreate();
                database.Execute(script);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Info(ex.ToString());

                return false;
            }

            return true;
        }

        private static Database CreateDatabase(CommandLine cmd)
        {
            var database = new Database
            {
                ConnectionString = cmd.Connection.ToString(),
                Log = Logger,
                Configuration = AppConfiguration.GetCurrent(),
                Transaction = cmd.Transaction
            };

            foreach (var entry in cmd.Variables)
            {
                database.Variables.SetValue(entry.Key, entry.Value);
            }

            return database;
        }

        private static CommandLine ParseCommandLine(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            try
            {
                return CommandLineBuilder.FromArguments(args);
            }
            catch (Exception e)
            {
                Logger.Error("Invalid command line: {0}".FormatWith(e.Message));
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