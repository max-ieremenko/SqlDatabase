using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    [TestFixture]
    public class CommandFactoryTest
    {
        private CommandFactory _sut;
        private ILogger _log;
        private CommandLine _commandLine;

        [SetUp]
        public void BeforeEachTest()
        {
            _commandLine = new CommandLine
            {
                Connection = new SqlConnectionStringBuilder()
            };

            _log = new Mock<ILogger>(MockBehavior.Strict).Object;
            _sut = new CommandFactory { Log = _log };
        }

        [Test]
        public void ResolveCreateCommand()
        {
            _commandLine.Command = Command.Create;

            var actual = _sut.Resolve(_commandLine);
            Assert.IsInstanceOf<DatabaseCreateCommand>(actual);
            Assert.AreEqual(_log, actual.Log);
            Assert.IsInstanceOf<Database>(actual.Database);

            var database = (Database)actual.Database;
            Assert.IsNotNull(database.Configuration);

            var command = (DatabaseCreateCommand)actual;
            Assert.IsInstanceOf<CreateScriptSequence>(command.ScriptSequence);
        }

        [Test]
        public void ResolveUpgradeCommand()
        {
            _commandLine.Command = Command.Upgrade;

            var actual = _sut.Resolve(_commandLine);
            Assert.IsInstanceOf<DatabaseUpgradeCommand>(actual);
            Assert.AreEqual(_log, actual.Log);
            Assert.IsInstanceOf<Database>(actual.Database);

            var database = (Database)actual.Database;
            Assert.IsNotNull(database.Configuration);

            var command = (DatabaseUpgradeCommand)actual;
            Assert.IsInstanceOf<UpgradeScriptSequence>(command.ScriptSequence);
        }

        [Test]
        public void ResolveExecuteCommand()
        {
            _commandLine.Command = Command.Execute;
            _commandLine.Scripts.Add(GetType().Assembly.Location);

            var actual = _sut.Resolve(_commandLine);
            Assert.IsInstanceOf<DatabaseExecuteCommand>(actual);
            Assert.AreEqual(_log, actual.Log);
            Assert.IsInstanceOf<Database>(actual.Database);

            var database = (Database)actual.Database;
            Assert.IsNotNull(database.Configuration);

            var command = (DatabaseExecuteCommand)actual;
            Assert.IsInstanceOf<Database>(command.Database);
        }

        [Test]
        public void ResolveUnknownCommand()
        {
            _commandLine.Command = (Command)100;

            Assert.Throws<NotImplementedException>(() => _sut.Resolve(_commandLine));
        }
    }
}
