using System;
using System.Runtime.Serialization;

namespace SqlDatabase.Configuration;

[Serializable]
public class InvalidCommandLineException : SystemException
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

    protected InvalidCommandLineException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Argument = info.GetString(nameof(Argument));
    }

    public string Argument { get; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(Argument), Argument);
    }
}