using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineParserTest
    {
        private CommandLineParser _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new CommandLineParser();
        }

        [Test]
        [TestCase("-x=y", "x", "y", true)]
        [TestCase("x=y", null, "x=y", true)]
        [TestCase("-x", "x", null, true)]
        [TestCase("-x=", "x", null, true)]
        [TestCase("-=x", null, null, false)]
        [TestCase("-=", null, null, false)]
        [TestCase("-", null, null, false)]
        public void SplitArg(string keyValue, string expectedKey, string expectedValue, bool isValid)
        {
            CommandLineParser.ParseArg(keyValue, out var actual).ShouldBe(isValid);
            if (isValid)
            {
                if (expectedKey == null)
                {
                    actual.IsPair.ShouldBeFalse();

                    actual.Value.ShouldBe(expectedValue);
                }
                else
                {
                    actual.IsPair.ShouldBeTrue();

                    actual.Key.ShouldBe(expectedKey);
                    actual.Value.ShouldBe(expectedValue);
                }
            }
        }

        [Test]
        public void Parse()
        {
            var actual = _sut
                .Parse(
                    "execute",
                    "create",
                    "-database=connection string",
                    "-from=folder 1",
                    "-from=folder 2")
                .Args;

            actual.Count.ShouldBe(5);

            actual[0].IsPair.ShouldBeFalse();
            actual[0].Value.ShouldBe("execute");

            actual[1].IsPair.ShouldBeFalse();
            actual[1].Value.ShouldBe("create");

            actual[2].IsPair.ShouldBeTrue();
            actual[2].Key.ShouldBe("database");
            actual[2].Value.ShouldBe("connection string");

            actual[3].IsPair.ShouldBeTrue();
            actual[3].Key.ShouldBe("from");
            actual[3].Value.ShouldBe("folder 1");

            actual[4].IsPair.ShouldBeTrue();
            actual[4].Key.ShouldBe("from");
            actual[4].Value.ShouldBe("folder 2");
        }

        [Test]
        public void ParseFail()
        {
            Assert.Throws<InvalidCommandLineException>(() => _sut.Parse("-"));
        }
    }
}
