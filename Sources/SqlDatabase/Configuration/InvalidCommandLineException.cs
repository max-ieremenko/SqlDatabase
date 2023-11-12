using System;
using System.Runtime.Serialization;

namespace SqlDatabase.Configuration;

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

    public InvalidCommandLineException(string argument, string message)
        : base(message)
    {
        Argument = argument;
    }

    public InvalidCommandLineException(string argument, string message, Exception inner)
        : base(message, inner)
    {
        Argument = argument;
    }

    public string? Argument { get; }
}