using System;
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

            var factory = ResolveFactory(args, logger);
            if (factory == null)
            {
                logger.Info(LoadHelpContent("CommandLine.txt"));
                return ExitCode.InvalidCommandLine;
            }

            ICommandLine cmd;
            if (factory.ShowCommandHelp || (cmd = ResolveCommandLine(factory, logger)) == null)
            {
                logger.Info(LoadHelpContent("CommandLine." + factory.ActiveCommandName + ".txt"));
                return ExitCode.InvalidCommandLine;
            }

            var exitCode = ExecuteCommand(cmd, logger) ? ExitCode.Ok : ExitCode.ExecutionErrors;
            return exitCode;
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

        private static ICommandLine ResolveCommandLine(CommandLineFactory factory, ILogger logger)
        {
            try
            {
                return factory.Resolve();
            }
            catch (Exception e)
            {
                logger.Error("Invalid command line: {0}".FormatWith(e.Message));
            }

            return null;
        }

        private static CommandLineFactory ResolveFactory(string[] args, ILogger logger)
        {
            try
            {
                var command = new CommandLineParser().Parse(args);
                if (command.Args.Count == 0)
                {
                    return null;
                }

                var factory = new CommandLineFactory { Args = command };
                return factory.Bind() ? factory : null;
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

        private static string LoadHelpContent(string fileName)
        {
            var scope = typeof(ICommandLine);
            using (var stream = scope.Assembly.GetManifestResourceStream(scope, fileName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}