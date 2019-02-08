using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class OutputReaderTest
    {
        private OutputReader _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new OutputReader();
        }

        [Test]
        public void ReadInfo()
        {
            var line = NextLine("some text");

            line.Key.ShouldBe("some text");
            line.Value.ShouldBe(false);
        }

        [Test]
        public void ReadEmptyInfo()
        {
            var line = NextLine(string.Empty);

            line.Key.ShouldBe(string.Empty);
            line.Value.ShouldBe(false);
        }

        [Test]
        public void ReadError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "some error", OutputReader.SetForegroundColorToDefault);

            line.Key.ShouldBe("some error");
            line.Value.ShouldBe(true);
        }

        [Test]
        public void ReadEmptyError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, string.Empty, OutputReader.SetForegroundColorToDefault);

            line.Key.ShouldBe(string.Empty);
            line.Value.ShouldBe(true);
        }

        [Test]
        public void ReadMixed()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "error 1");

            line.Key.ShouldBe("error 1");
            line.Value.ShouldBe(true);

            line = NextLine("error 2", OutputReader.SetForegroundColorToDefault);

            line.Key.ShouldBe("error 2");
            line.Value.ShouldBe(true);

            line = NextLine("message");

            line.Key.ShouldBe("message");
            line.Value.ShouldBe(false);
        }

        private KeyValuePair<string, bool> NextLine(params string[] values)
        {
            var line = string.Join(string.Empty, values);
            return _sut.NextLine(line);
        }
    }
}
