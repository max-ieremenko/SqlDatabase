using System.Data.SqlClient;
using NUnit.Framework;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineBuilderTest
    {
        private CommandLineBuilder _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new CommandLineBuilder();
        }

        [Test]
        [TestCase("create", Command.Create)]
        [TestCase("Upgrade", Command.Upgrade)]
        [TestCase("ExecutE", Command.Execute)]
        public void SetCommand(string commandName, Command expected)
        {
            _sut.SetCommand(commandName);
            Assert.AreEqual(expected, _sut.Line.Command, commandName);
        }

        [Test]
        [TestCase("Unknown")]
        [TestCase("")]
        [TestCase("upgrade1")]
        public void SetInvalidCommand(string commandName)
        {
            var ex = Assert.Throws<InvalidCommandException>(() => _sut.SetCommand(commandName));
            Assert.AreEqual(commandName, ex.Argument);
        }

        [Test]
        public void CreateDoesNotSupportTransaction()
        {
            _sut.Line.Connection = new SqlConnectionStringBuilder();
            _sut.Line.Scripts.Add("does not matter");

            _sut
                .SetCommand(Command.Create)
                .SetTransaction(TransactionMode.PerStep);

            var ex = Assert.Throws<InvalidCommandException>(() => _sut.Build());

            Assert.AreEqual("-transaction", ex.Argument);
        }

        [Test]
        public void FromArgumentsCreate()
        {
            var actual = CommandLineBuilder.FromArguments(
                "create",
                "-database=Data Source=SQL2016DEV;Initial Catalog=test",
                @"-from=c:\folder",
                "-varX=1 2 3",
                "-varY=value",
                "-configuration=app.config");

            Assert.AreEqual(Command.Create, actual.Command);
            StringAssert.AreEqualIgnoringCase(@"c:\folder", actual.Scripts[0]);

            Assert.IsNotNull(actual.Connection);
            StringAssert.AreEqualIgnoringCase("test", actual.Connection.InitialCatalog);
            StringAssert.AreEqualIgnoringCase("SQL2016DEV", actual.Connection.DataSource);

            Assert.AreEqual(TransactionMode.None, actual.Transaction);

            CollectionAssert.AreEquivalent(new[] { "X", "Y" }, actual.Variables.Keys);
            Assert.AreEqual("1 2 3", actual.Variables["x"]);
            Assert.AreEqual("value", actual.Variables["y"]);

            Assert.AreEqual("app.config", actual.ConfigurationFile);
        }

        [Test]
        public void FromArgumentsUpgrade()
        {
            var actual = CommandLineBuilder.FromArguments(
                "upgrade",
                "-database=Data Source=SQL2016DEV;Initial Catalog=test",
                @"-from=c:\folder",
                "-transaction=PerStep",
                "-varX=1 2 3",
                "-varY=value");

            Assert.AreEqual(Command.Upgrade, actual.Command);
            StringAssert.AreEqualIgnoringCase(@"c:\folder", actual.Scripts[0]);

            Assert.IsNotNull(actual.Connection);
            StringAssert.AreEqualIgnoringCase("test", actual.Connection.InitialCatalog);
            StringAssert.AreEqualIgnoringCase("SQL2016DEV", actual.Connection.DataSource);

            Assert.AreEqual(TransactionMode.PerStep, actual.Transaction);

            CollectionAssert.AreEquivalent(new[] { "X", "Y" }, actual.Variables.Keys);
            Assert.AreEqual("1 2 3", actual.Variables["x"]);
            Assert.AreEqual("value", actual.Variables["y"]);

            Assert.IsNull(actual.ConfigurationFile);
        }

        [Test]
        public void FromArgumentsExecute()
        {
            var actual = CommandLineBuilder.FromArguments(
                "execute",
                "-database=Data Source=SQL2016DEV;Initial Catalog=test",
                @"-from=c:\folder\11.sql",
                "-transaction=perStep",
                "-varX=1 2 3",
                "-varY=value");

            Assert.AreEqual(Command.Execute, actual.Command);
            StringAssert.AreEqualIgnoringCase(@"c:\folder\11.sql", actual.Scripts[0]);

            Assert.IsNotNull(actual.Connection);
            StringAssert.AreEqualIgnoringCase("test", actual.Connection.InitialCatalog);
            StringAssert.AreEqualIgnoringCase("SQL2016DEV", actual.Connection.DataSource);

            Assert.AreEqual(TransactionMode.PerStep, actual.Transaction);

            CollectionAssert.AreEquivalent(new[] { "X", "Y" }, actual.Variables.Keys);
            Assert.AreEqual("1 2 3", actual.Variables["x"]);
            Assert.AreEqual("value", actual.Variables["y"]);

            Assert.IsNull(actual.ConfigurationFile);
        }

        [Test]
        public void SeveralFromArguments()
        {
            var actual = CommandLineBuilder.FromArguments(
                "execute",
                "-database=Data Source=SQL2016DEV;Initial Catalog=test",
                @"-from=c:\folder\11.sql",
                @"-from=c:\folder\subFolder",
                @"-from=c:\folder\11.zip");

            Assert.AreEqual(3, actual.Scripts.Count);

            StringAssert.AreEqualIgnoringCase(@"c:\folder\11.sql", actual.Scripts[0]);
            StringAssert.AreEqualIgnoringCase(@"c:\folder\subFolder", actual.Scripts[1]);
            StringAssert.AreEqualIgnoringCase(@"c:\folder\11.zip", actual.Scripts[2]);
        }

        [Test]
        public void SetInvalidConnection()
        {
            var ex = Assert.Throws<InvalidCommandException>(() => _sut.SetConnection("-database=xxx"));
            Assert.AreEqual("-database", ex.Argument);
        }
    }
}