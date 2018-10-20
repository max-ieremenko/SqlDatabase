using System;
using System.Diagnostics;
using System.IO;
using SqlDatabase.Commands;
using SqlDatabase.Configuration;

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
            try
            {
                var factory = new CommandFactory { Log = Logger };
                factory.Resolve(cmd).Execute();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Info(ex.ToString());
                return false;
            }

            throw new NotImplementedException();
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