using System;
using System.Collections.Generic;
using SqlDatabase.Configuration;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class CommandFactory
    {
        public ILogger Log { get; set; }

        public DatabaseCommandBase Resolve(CommandLine commandLine)
        {
            switch (commandLine.Command)
            {
                case Command.Upgrade:
                    return ResolveUpgradeCommand(commandLine);
                case Command.Create:
                    return ResolveCreateCommand(commandLine);
                case Command.Execute:
                    return ResolveExecuteCommand(commandLine);
            }

            throw new NotImplementedException("Unexpected command type [{0}].".FormatWith(commandLine.Command));
        }

        private static void FillSources(IList<IFileSystemInfo> sources, IList<string> scripts)
        {
            foreach (var script in scripts)
            {
                sources.Add(FileSystemFactory.FileSystemInfoFromPath(script));
            }
        }

        private DatabaseExecuteCommand ResolveExecuteCommand(CommandLine commandLine)
        {
            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory()
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseExecuteCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                ScriptSequence = sequence
            };
        }

        private DatabaseUpgradeCommand ResolveUpgradeCommand(CommandLine commandLine)
        {
            var sequence = new UpgradeScriptSequence
            {
                ScriptFactory = new ScriptFactory()
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseUpgradeCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                ScriptSequence = sequence
            };
        }

        private DatabaseCreateCommand ResolveCreateCommand(CommandLine commandLine)
        {
            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory()
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseCreateCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                ScriptSequence = sequence
            };
        }

        private Database CreateDatabase(CommandLine cmd)
        {
            var database = new Database
            {
                ConnectionString = cmd.Connection.ToString(),
                Log = Log,
                Configuration = AppConfiguration.GetCurrent(),
                Transaction = cmd.Transaction
            };

            foreach (var entry in cmd.Variables)
            {
                database.Variables.SetValue(entry.Key, entry.Value);
            }

            return database;
        }
    }
}
