using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;
using SqlDatabase.Commands;
using SqlDatabase.Configuration;
using SqlDatabase.FileSystem;
using SqlDatabase.Log;

namespace SqlDatabase;

internal static class Program
{
    public static int Main(string[] args)
    {
        var logger = LoggerFactory.CreateDefault();

        try
        {
            var runtime = HostedRuntimeResolver.GetRuntime(false);

            if (CommandLineParser.HelpRequested(args, out var command))
            {
                logger.Info(LoadHelpContent(GetHelpFileName(runtime, command)));
                return ExitCode.InvalidCommandLine;
            }

            var commandLine = CommandLineParser.Parse(runtime, args);
            logger = LoggerFactory.WrapWithUsersLogger(logger, commandLine.Log);
            MainCore(logger, runtime, commandLine, Environment.CurrentDirectory);
        }
        catch (InvalidCommandLineException ex)
        {
            logger.Error($"Invalid command line: {ex.Message}.", ex);
            return ExitCode.InvalidCommandLine;
        }
        catch (Exception ex)
        {
            logger.Error("Something went wrong.", ex);
            return ExitCode.ExecutionErrors;
        }
        finally
        {
            (logger as IDisposable)?.Dispose();
        }

        return ExitCode.Ok;
    }

    internal static void RunPowershell(ILogger logger, ICommandLine commandLine, string currentDirectory)
    {
        logger = LoggerFactory.WrapWithUsersLogger(logger, commandLine.Log);
        try
        {
            var runtime = HostedRuntimeResolver.GetRuntime(true);
            MainCore(logger, runtime, commandLine, currentDirectory);
        }
        finally
        {
            (logger as IDisposable)?.Dispose();
        }
    }

    private static void MainCore(ILogger logger, HostedRuntime runtime, ICommandLine commandLine, string currentDirectory)
    {
        var fileSystem = new FileSystemFactory(currentDirectory);
        var factory = new CommandFactory(logger, new EnvironmentBuilder(runtime, fileSystem), fileSystem);
        var command = factory.CreateCommand(commandLine);
        command.Execute();
    }

    private static string GetHelpFileName(HostedRuntime runtime, string? commandName)
    {
        if (runtime.IsPowershell)
        {
            throw new NotSupportedException();
        }

        if (commandName == null)
        {
            return "CommandLine.txt";
        }

        var suffix = runtime.Version == FrameworkVersion.Net472 ? ".net472" : null;
        return "CommandLine." + commandName + suffix + ".txt";
    }

    private static string LoadHelpContent(string fileName)
    {
        var scope = typeof(IEnvironmentBuilder);

        // .net core resource name is case-sensitive
        var fullName = scope.Namespace + "." + fileName;
        var resourceName = scope
            .Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(i => string.Equals(fullName, i, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InvalidOperationException($"Help file [{fullName}] not found.");
        }

        using (var stream = scope.Assembly.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream!))
        {
            return reader.ReadToEnd();
        }
    }
}