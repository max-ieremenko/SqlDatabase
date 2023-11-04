#if NET472
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.AssemblyInternal.Net472
{
    [TestFixture]
    public partial class Net472SubDomainTest
    {
        private Net472SubDomain _sut;
        private Variables _variables;
        private Mock<IDbCommand> _command;

        private IList<string> _executedScripts;

        [SetUp]
        public void BeforeEachTest()
        {
            _variables = new Variables();

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m => Console.WriteLine("Info: {0}", m));
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m => Console.WriteLine("Error: {0}", m));

            _executedScripts = new List<string>();
            _command = new Mock<IDbCommand>(MockBehavior.Strict);
            _command.SetupProperty(c => c.CommandText);
            _command
                .Setup(c => c.ExecuteNonQuery())
                .Callback(() => _executedScripts.Add(_command.Object.CommandText))
                .Returns(0);

            _sut = new Net472SubDomain { Logger = log.Object };

            _sut.AssemblyFileName = GetType().Assembly.Location;
            _sut.ReadAssemblyContent = () => File.ReadAllBytes(GetType().Assembly.Location);

            _sut.Initialize();
        }

        [TearDown]
        public void AfterEachTest()
        {
            _sut?.Unload();
            _sut?.Dispose();
        }

        [Test]
        public void ValidateScriptDomainAppBase()
        {
            _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.ShowAppBase));
            _sut.Execute(new DbCommandStub(_command.Object), _variables);
            _sut.Unload();
            _sut.Dispose();

            _executedScripts.Count.ShouldBe(2);

            var assemblyFileName = _executedScripts[0];
            FileAssert.DoesNotExist(assemblyFileName);
            Path.GetFileName(GetType().Assembly.Location).ShouldBe(Path.GetFileName(assemblyFileName));

            var appBase = _executedScripts[1];
            DirectoryAssert.DoesNotExist(appBase);
            Path.GetDirectoryName(assemblyFileName).ShouldBe(appBase);
        }

        [Test]
        public void ValidateScriptDomainConfiguration()
        {
            _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.ShowConfiguration));
            _sut.Execute(new DbCommandStub(_command.Object), _variables);
            _sut.Unload();
            _sut.Dispose();

            Assert.AreEqual(2, _executedScripts.Count);

            var configurationFile = _executedScripts[0];
            configurationFile.ShouldBe(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            var connectionString = _executedScripts[1];
            connectionString.ShouldBe(MsSqlQuery.ConnectionString);
        }

        [Test]
        public void ValidateScriptDomainCreateSubDomain()
        {
            _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.Execute));
            _sut.Execute(new DbCommandStub(_command.Object), _variables);

            _executedScripts.Count.ShouldBe(1);
            _executedScripts[0].ShouldBe("hello");
        }
    }
}
#endif