using System;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class GenericCommandLineBuilderTest
    {
        private GenericCommandLineBuilder _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new GenericCommandLineBuilder();
        }

        [Test]
        public void EscapedCommandLine()
        {
            var escapedArgs = _sut
                .SetCommand("some command")
                .SetConnection("Data Source=.;Initial Catalog=SqlDatabaseTest")
                .SetScripts("file1")
                .SetScripts("file2")
                .SetConfigurationFile("configuration file")
                .SetTransaction(TransactionMode.PerStep)
                .SetVariable("var1", "value 1")
                .SetWhatIf(true)
                .SetFolderAsModuleName(true)
                .SetPreFormatOutputLogs(true)
                .BuildArray(true);

            foreach (var arg in escapedArgs)
            {
                Console.WriteLine(arg);
            }

            CommandLineParser.PreFormatOutputLogs(escapedArgs).ShouldBeTrue();
            var actual = new CommandLineParser().Parse(escapedArgs);

            actual.Args[0].IsPair.ShouldBe(false);
            actual.Args[0].Value.ShouldBe("some command");

            actual.Args[1].Key.ShouldBe("database");
            actual.Args[1].Value.ShouldBe("Data Source=.;Initial Catalog=SqlDatabaseTest");

            actual.Args[2].Key.ShouldBe("from");
            actual.Args[2].Value.ShouldBe("file1");

            actual.Args[3].Key.ShouldBe("from");
            actual.Args[3].Value.ShouldBe("file2");

            actual.Args[4].Key.ShouldBe("transaction");
            actual.Args[4].Value.ShouldBe("PerStep");

            actual.Args[5].Key.ShouldBe("configuration");
            actual.Args[5].Value.ShouldBe("configuration file");

            actual.Args[6].Key.ShouldBe("varvar1");
            actual.Args[6].Value.ShouldBe("value 1");

            actual.Args[7].Key.ShouldBe("whatIf");
            actual.Args[7].Value.ShouldBe("True");

            actual.Args[8].Key.ShouldBe("folderAsModuleName");
            actual.Args[8].Value.ShouldBe("True");
        }

        [Test]
        public void BuildArrayScripts()
        {
            var actual = _sut
                .SetScripts("file1")
                .SetScripts("file2")
                .BuildArray(false);

            actual.Length.ShouldBe(4);
            actual[2].ShouldBe("-from=file1");
            actual[3].ShouldBe("-from=file2");
        }

        [Test]
        public void BuildArrayInLineScripts()
        {
            var actual = _sut
                .SetInLineScript("file1")
                .SetInLineScript("file2")
                .BuildArray(false);

            actual.Length.ShouldBe(4);
            actual[2].ShouldBe("-fromSql=file1");
            actual[3].ShouldBe("-fromSql=file2");
        }

        [Test]
        public void BuildArrayExportToTable()
        {
            var actual = _sut
                .SetExportToTable("name")
                .BuildArray(false);

            actual.Length.ShouldBe(3);
            actual[2].ShouldBe("-toTable=name");
        }

        [Test]
        public void BuildArrayExportToFile()
        {
            var actual = _sut
                .SetExportToFile("file name")
                .BuildArray(false);

            actual.Length.ShouldBe(3);
            actual[2].ShouldBe("-toFile=file name");
        }
    }
}