using SqlDatabase.Adapter;

namespace SqlDatabase.Log;

internal static class LoggerFactory
{
    public static ILogger CreateDefault()
    {
        return new ConsoleLogger();
    }

    public static ILogger WrapWithUsersLogger(ILogger logger, string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return logger;
        }

        ILogger fileLogger;
        try
        {
            fileLogger = new FileLogger(fileName!);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Fail to create file log.", ex);
        }

        return new CombinedLogger(logger, false, fileLogger, true);
    }
}