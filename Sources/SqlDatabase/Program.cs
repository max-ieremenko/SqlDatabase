using System;
using System.Diagnostics;
using System.IO;
using SqlDatabase.Commands;
using SqlDatabase.Configuration;
using SqlDatabase.Log;

namespace SqlDatabase
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var logger = CommandLineBuilder.PreFormatOutputLogs(args) ?
                LoggerFactory.CreatePreFormatted() :
                LoggerFactory.CreateDefault();

            ExitCode exitCode;
            var cmd = ParseCommandLine(args, logger);
            if (cmd == null)
            {
                logger.Info(LoadHelpContent());
                exitCode = ExitCode.InvalidCommandLine;
            }
            else if (cmd.Command == Command.Echo)
            {
                foreach (var arg in args)
                {
                    logger.Info(arg);
                }

                exitCode = ExitCode.Ok;
            }
            else
            {
                exitCode = ExecuteCommand(cmd, logger) ? ExitCode.Ok : ExitCode.ExecutionErrors;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            return (int)exitCode;
        }

        private static bool ExecuteCommand(CommandLine cmd, ILogger logger)
        {
            try
            {
                var factory = new CommandFactory { Log = logger };
                factory.Resolve(cmd).Execute();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Info(ex.ToString());
                return false;
            }
        }

        private static CommandLine ParseCommandLine(string[] args, ILogger logger)
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
                logger.Error("Invalid command line: {0}".FormatWith(e.Message));
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