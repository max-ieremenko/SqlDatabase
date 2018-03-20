using NUnit.Framework;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineTest
    {
        [Test]
        public void ParseUpgrade()
        {
            var actual = CommandLine.Parse(
                "upgrade",
                "-database=Data Source=SQL2016DEV;Initial Catalog=test",
                @"-from=c:\folder",
                "-transaction=PerStep",
                "-varX=1 2 3",
                "-varY=value");

            Assert.AreEqual(Command.Upgrade, actual.Command);
            StringAssert.AreEqualIgnoringCase(@"c:\folder", actual.Scripts);

            Assert.IsNotNull(actual.Connection);
            StringAssert.AreEqualIgnoringCase("test", actual.Connection.InitialCatalog);
            StringAssert.AreEqualIgnoringCase("SQL2016DEV", actual.Connection.DataSource);

            Assert.AreEqual(TransactionMode.PerStep, actual.Transaction);

            CollectionAssert.AreEquivalent(new[] { "X", "Y" }, actual.Variables.Keys);
            Assert.AreEqual("1 2 3", actual.Variables["x"]);
            Assert.AreEqual("value", actual.Variables["y"]);
        }

        [Test]
        [TestCase("Unknown")]
        [TestCase("")]
        [TestCase("upgrade1")]
        public void ParseInvalidCommand(string command)
        {
            var ex = Assert.Throws<InvalidCommandException>(() => CommandLine.Parse(command));
            Assert.AreEqual(command, ex.Argument);
        }

        [Test]
        public void ParseInvalidConnectionString()
        {
            var ex = Assert.Throws<InvalidCommandException>(() => CommandLine.Parse("upgrade", "-database=xxx"));
            Assert.AreEqual("xxx", ex.Argument);
        }
    }
}