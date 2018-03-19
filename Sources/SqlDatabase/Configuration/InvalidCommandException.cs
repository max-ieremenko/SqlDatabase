using System;
using System.Runtime.Serialization;

namespace SqlDatabase.Configuration
{
    [Serializable]
    public class InvalidCommandException : SystemException
    {
        public InvalidCommandException()
        {
        }

        public InvalidCommandException(string message)
            : base(message)
        {
        }

        public InvalidCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidCommandException(string argument, string message)
            : base(message)
        {
            Argument = argument;
        }

        public InvalidCommandException(string argument, string message, Exception inner)
            : base(message, inner)
        {
            Argument = argument;
        }

        protected InvalidCommandException(SerializationInfo info, StreamingContext context)
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
}