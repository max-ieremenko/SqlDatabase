using System;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineFactoryTest
    {
        private CommandLineFactory _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new CommandLineFactory();
        }

        [Test]
        [TestCase(CommandLineFactory.CommandCreate, typeof(CreateCommandLine))]
        [TestCase(CommandLineFactory.CommandUpgrade, typeof(UpgradeCommandLine))]
        [TestCase(CommandLineFactory.CommandExecute, typeof(ExecuteCommandLine))]
        [TestCase(CommandLineFactory.CommandExport, typeof(ExportCommandLine))]
        [TestCase(CommandLineFactory.CommandEcho, typeof(EchoCommandLine))]
        public void Bind(string command, Type commandLine)
        {
            _sut.Args = new CommandLine(new Arg(command));

            _sut.Bind().ShouldBeTrue();

            _sut.ActiveCommandName.ShouldBe(command);
            _sut.ShowCommandHelp.ShouldBeFalse();

            CommandLineFactory.CreateCommand(_sut.ActiveCommandName).ShouldBeOfType(commandLine);
        }
    }
}
