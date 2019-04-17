using System;
using System.Diagnostics;
using System.IO;
using SqlDatabase.Configuration;
using SqlDatabase.Log;

namespace SqlDatabase
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var logger = CreateLogger(args);

            ExitCode exitCode;
            var cmd = ParseCommandLine(args, logger);
            if (cmd == null)
            {
                logger.Info(LoadHelpContent());
                exitCode = ExitCode.InvalidCommandLine;
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

        private static bool ExecuteCommand(ICommandLine cmd, ILogger logger)
        {
            try
            {
                cmd.CreateCommand(logger).Execute();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Info(ex.ToString());
                return false;
            }
        }

        private static ICommandLine ParseCommandLine(string[] args, ILogger logger)
        {
            try
            {
                var command = new CommandLineParser().Parse(args);
                if (command.Args.Count == 0)
                {
                    return null;
                }

                return new CommandLineFactory().Resolve(command);
            }
            catch (Exception e)
            {
                logger.Error("Invalid command line: {0}".FormatWith(e.Message));
            }

            return null;
        }

        private static ILogger CreateLogger(string[] args)
        {
            return CommandLineParser.PreFormatOutputLogs(args) ?
                LoggerFactory.CreatePreFormatted() :
                LoggerFactory.CreateDefault();
        }

        private static string LoadHelpContent()
        {
            var scope = typeof(ICommandLine);
            using (var stream = scope.Assembly.GetManifestResourceStream(scope, "CommandLine.txt"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}