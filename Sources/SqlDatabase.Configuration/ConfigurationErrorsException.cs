namespace SqlDatabase.Configuration;

public sealed class ConfigurationErrorsException : ApplicationException
{
    public ConfigurationErrorsException()
    {
    }

    public ConfigurationErrorsException(string message)
        : base(message)
    {
    }

    public ConfigurationErrorsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}