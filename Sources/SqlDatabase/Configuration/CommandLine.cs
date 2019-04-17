using System.Collections.Generic;

namespace SqlDatabase.Configuration
{
    internal struct CommandLine
    {
        public CommandLine(IList<Arg> args, string[] original)
        {
            Args = args;
            Original = original;
        }

        public CommandLine(params Arg[] args)
        {
            Args = args;
            Original = null;
        }

        public IList<Arg> Args { get; }

        public string[] Original { get; }
    }
}
