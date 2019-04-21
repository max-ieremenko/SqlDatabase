using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class ExportCmdLetTest
    {
        private ExportCmdLet _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new ExportCmdLet();
        }

        [Test]
        public void BuildCommandLine()
        {
            _sut.ToTable = "table 1";
            _sut.ToFile = "file name";

            var cmd = new GenericCommandLineBuilder();
            _sut.BuildCommandLine(cmd);

            cmd.Line.ExportToTable.ShouldBe("table 1");
            cmd.Line.ExportToFile.ShouldBe("file name");
        }
    }
}
