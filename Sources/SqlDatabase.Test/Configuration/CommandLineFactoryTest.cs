using System;
using System.Collections.Generic;
using System.Linq;
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
        public void FindCommand(string command, Type commandLine)
        {
            var commandArgs = new List<Arg> { new Arg(command) };

            _sut.FindCommand(commandArgs).ShouldBeOfType(commandLine);

            commandArgs.Count.ShouldBe(0);
        }

        [Test]
        [TestCase("unknown command")]
        [TestCase("execute", "create")]
        [TestCase("execute", "execute")]
        public void FailToFindCommand(params string[] args)
        {
            var commandArgs = args.Select(i => new Arg(i)).ToList();

            Assert.Throws<InvalidCommandLineException>(() => _sut.FindCommand(commandArgs));
        }
    }
}
