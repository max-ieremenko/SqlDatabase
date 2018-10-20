using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    [TestFixture]
    public class DatabaseExecuteCommandTest
    {
        private DatabaseExecuteCommand _sut;
        private Mock<IDatabase> _database;
        private Mock<IScript> _script;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<IDatabase>(MockBehavior.Strict);
            _database.SetupGet(d => d.ConnectionString).Returns(@"Data Source=unknownServer;Initial Catalog=unknownDatabase");
            _database.Setup(d => d.GetServerVersion()).Returns("sql server 1.0");

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                });

            _script = new Mock<IScript>(MockBehavior.Strict);

            _sut = new DatabaseExecuteCommand
            {
                Database = _database.Object,
                Log = log.Object,
                Script = _script.Object
            };
        }

        [Test]
        public void ExecuteOneScript()
        {
            _script.SetupGet(s => s.DisplayName).Returns("step");

            _database.Setup(d => d.Execute(_script.Object));

            _sut.Execute();

            _database.VerifyAll();
            _script.VerifyAll();
        }
    }
}
