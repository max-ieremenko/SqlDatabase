namespace SqlDatabase.CommandLine;

public sealed class InvalidCommandLineException : SystemException
{
    public InvalidCommandLineException()
    {
    }

    public InvalidCommandLineException(string message)
        : base(message)
    {
    }

    public InvalidCommandLineException(string message, Exception inner)
        : base(message, inner)
    {
    }
}