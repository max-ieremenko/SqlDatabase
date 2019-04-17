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
        public void FindCommandCreateCommandLine()
        {
            var commandArgs = new List<Arg> { new Arg("create") };

            _sut.FindCommand(commandArgs).ShouldBeOfType<CreateCommandLine>();

            commandArgs.Count.ShouldBe(0);
        }

        [Test]
        public void FindCommandExecuteCommandLine()
        {
            var commandArgs = new List<Arg> { new Arg("execute") };

            _sut.FindCommand(commandArgs).ShouldBeOfType<ExecuteCommandLine>();

            commandArgs.Count.ShouldBe(0);
        }

        [Test]
        public void FindCommandUpgradeCommandLine()
        {
            var commandArgs = new List<Arg> { new Arg("upgrade") };

            _sut.FindCommand(commandArgs).ShouldBeOfType<UpgradeCommandLine>();

            commandArgs.Count.ShouldBe(0);
        }

        [Test]
        public void FindCommandEchoCommandLine()
        {
            var commandArgs = new List<Arg> { new Arg("echo") };

            _sut.FindCommand(commandArgs).ShouldBeOfType<EchoCommandLine>();

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
