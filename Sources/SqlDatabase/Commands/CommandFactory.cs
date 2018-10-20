using System;
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

        private DatabaseExecuteCommand ResolveExecuteCommand(CommandLine commandLine)
        {
            var file = FileSytemFactory.FileFromPath(commandLine.Scripts);

            return new DatabaseExecuteCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                Script = new ScriptFactory().FromFile(file)
            };
        }

        private DatabaseUpgradeCommand ResolveUpgradeCommand(CommandLine commandLine)
        {
            return new DatabaseUpgradeCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                ScriptSequence = new UpgradeScriptSequence
                {
                    Root = FileSytemFactory.FolderFromPath(commandLine.Scripts),
                    ScriptFactory = new ScriptFactory()
                }
            };
        }

        private DatabaseCreateCommand ResolveCreateCommand(CommandLine commandLine)
        {
            return new DatabaseCreateCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine),
                ScriptSequence = new CreateScriptSequence
                {
                    Root = FileSytemFactory.FolderFromPath(commandLine.Scripts),
                    ScriptFactory = new ScriptFactory()
                }
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
