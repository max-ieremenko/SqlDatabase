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

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe("some text");
            line.Value.IsError.ShouldBeFalse();
        }

        [Test]
        public void ReadEmptyInfo()
        {
            var line = NextLine(string.Empty);

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe(string.Empty);
            line.Value.IsError.ShouldBeFalse();
        }

        [Test]
        public void ReadError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "some error", OutputReader.SetForegroundColorToDefault);

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe("some error");
            line.Value.IsError.ShouldBeTrue();
        }

        [Test]
        public void ReadEmptyError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, string.Empty, OutputReader.SetForegroundColorToDefault);

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe(string.Empty);
            line.Value.IsError.ShouldBeTrue();
        }

        [Test]
        public void ReadMixed()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "error 1");

            line.HasValue.ShouldBeFalse();

            line = NextLine("error 2", OutputReader.SetForegroundColorToDefault);

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe("error 1\r\nerror 2");
            line.Value.IsError.ShouldBeTrue();

            line = NextLine("message");

            line.HasValue.ShouldBeTrue();
            line.Value.Text.ShouldBe("message");
            line.Value.IsError.ShouldBeFalse();
        }

        [Test]
        public void BufferError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "line 1");
            line.HasValue.ShouldBeFalse();

            line = NextLine("line 2");
            line.HasValue.ShouldBeFalse();

            line = NextLine("line 3", OutputReader.SetForegroundColorToDefault);

            line.HasValue.ShouldBeTrue();
            line.Value.IsError.ShouldBeTrue();
            line.Value.Text.ShouldBe(@"line 1
line 2
line 3");
        }

        [Test]
        public void FlushAfterInfo()
        {
            NextLine("some text");

            _sut.Flush().HasValue.ShouldBeFalse();
        }

        [Test]
        public void FlushAfterError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "some text", OutputReader.SetForegroundColorToDefault);

            line.HasValue.ShouldBeTrue();
            line.Value.IsError.ShouldBeTrue();

            _sut.Flush().HasValue.ShouldBeFalse();
        }

        [Test]
        public void FlushBufferedError()
        {
            var line = NextLine(OutputReader.SetForegroundColorToRed, "some text");

            line.HasValue.ShouldBeFalse();

            line = _sut.Flush();

            line.HasValue.ShouldBeTrue();
            line.Value.IsError.ShouldBeTrue();
            line.Value.Text.ShouldBe("some text");
        }

        private OutputReader.Record? NextLine(params string[] values)
        {
            var line = string.Join(string.Empty, values);
            return _sut.NextLine(line);
        }
    }
}
