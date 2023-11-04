using System;
using System.IO;
using System.Linq;
using SqlDatabase.Configuration;
using SqlDatabase.Log;

namespace SqlDatabase;

internal static class Program
{
    public static int Main(string[] args)
    {
        var logger = LoggerFactory.CreateDefault();
        return Run(logger, args);
    }

    internal static int Run(ILogger logger, string[] args)
    {
        if (!TryWrapWithUsersLogger(logger, args, out var userLogger))
        {
            return ExitCode.InvalidCommandLine;
        }

        try
        {
            return MainCore(userLogger ?? logger, args);
        }
        finally
        {
            userLogger?.Dispose();
        }
    }

    private static int MainCore(ILogger logger, string[] args)
    {
        var factory = ResolveFactory(args, logger);
        if (factory == null)
        {
            logger.Info(LoadHelpContent("CommandLine.txt"));
            return ExitCode.InvalidCommandLine;
        }

        if (factory.ShowCommandHelp)
        {
            logger.Info(LoadHelpContent(GetHelpFileName(factory.ActiveCommandName)));
            return ExitCode.InvalidCommandLine;
        }

        var cmd = ResolveCommandLine(factory, logger);
        if (cmd == null)
        {
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
            logger.Error(ex);
            return false;
        }
    }

    private static ICommandLine ResolveCommandLine(CommandLineFactory factory, ILogger logger)
    {
        try
        {
            return factory.Resolve();
        }
        catch (Exception ex)
        {
            logger.Error("Invalid command line.", ex);
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
        catch (Exception ex)
        {
            logger.Error("Invalid command line.", ex);
        }

        return null;
    }

    private static bool TryWrapWithUsersLogger(ILogger logger, string[] args, out CombinedLogger combined)
    {
        combined = null;
        var fileName = CommandLineParser.GetLogFileName(args);
        if (string.IsNullOrEmpty(fileName))
        {
            return true;
        }

        ILogger fileLogger;
        try
        {
            fileLogger = new FileLogger(fileName);
        }
        catch (Exception ex)
        {
            logger.Error("Fail to create file log.", ex);
            return false;
        }

        combined = new CombinedLogger(logger, false, fileLogger, true);
        return true;
    }

    private static string GetHelpFileName(string commandName)
    {
#if NET472
        const string Runtime = ".net472";
#else
        const string Runtime = null;
#endif
        return "CommandLine." + commandName + Runtime + ".txt";
    }

    private static string LoadHelpContent(string fileName)
    {
        var scope = typeof(ICommandLine);

        // .net core resource name is case-sensitive
        var fullName = scope.Namespace + "." + fileName;
        var resourceName = scope
            .Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(i => string.Equals(fullName, i, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InvalidOperationException("Help file [{0}] not found.".FormatWith(fullName));
        }

        using (var stream = scope.Assembly.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}