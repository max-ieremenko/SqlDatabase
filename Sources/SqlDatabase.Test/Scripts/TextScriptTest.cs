using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts;

[TestFixture]
public class TextScriptTest
{
    private Mock<ILogger> _logger = null!;
    private Variables _variables = null!;
    private Mock<IDbCommand> _command = null!;
    private TextScript _sut = null!;

    private IList<string> _logOutput = null!;
    private IList<string> _executedScripts = null!;
    private Mock<IDataReader> _executedReader = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Variables();

        _logOutput = new List<string>();
        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _executedScripts = new List<string>();
        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _command.SetupProperty(c => c.CommandText);

        _executedReader = new Mock<IDataReader>(MockBehavior.Strict);
        _executedReader
            .Setup(r => r.Dispose());

        _command
            .Setup(c => c.ExecuteReader())
            .Callback(() => _executedScripts.Add(_command.Object.CommandText))
            .Returns(_executedReader.Object);

        _sut = new TextScript(null!, null!, new MsSqlTextReader());
        _variables.SetValue(VariableSource.CommandLine, "var1", "[some value]");
    }

    [Test]
    public void ExecuteShowVariableReplacement()
    {
        _sut.ReadSqlContent = "{{var1}} {{var1}}".AsFuncStream();

        _executedReader
            .Setup(r => r.GetSchemaTable())
            .Returns((DataTable)null!);
        _executedReader
            .Setup(r => r.Read())
            .Returns(false);
        _executedReader
            .Setup(r => r.NextResult())
            .Returns(false);

        _sut.Execute(_command.Object, _variables, _logger.Object);

        _executedScripts.Count.ShouldBe(1);
        _executedScripts[0].ShouldBe("[some value] [some value]");

        _logOutput.FirstOrDefault(i => i.Contains("var1") && i.Contains("[some value]")).ShouldNotBeNull();

        _executedReader.VerifyAll();
    }

    [Test]
    public void Execute()
    {
        _sut.ReadSqlContent = @"
{{var1}}
go
text2
go"
            .AsFuncStream();

        _executedReader
            .Setup(r => r.GetSchemaTable())
            .Returns((DataTable)null!);
        _executedReader
            .Setup(r => r.Read())
            .Returns(false);
        _executedReader
            .Setup(r => r.NextResult())
            .Returns(false);

        _sut.Execute(_command.Object, _variables, _logger.Object);

        _executedScripts.Count.ShouldBe(2);
        _executedScripts[0].ShouldBe("[some value]");
        _executedScripts[1].ShouldBe("text2");

        _executedReader.VerifyAll();
    }

    [Test]
    public void ExecuteWhatIf()
    {
        _sut.ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes(@"
{{var1}}
go
text2
go"));

        _sut.Execute(null, _variables, _logger.Object);

        _executedScripts.ShouldBeEmpty();
    }

    [Test]
    public void ExecuteReader()
    {
        _sut.ReadSqlContent = "select {{var1}}".AsFuncStream();

        var actual = _sut.ExecuteReader(_command.Object, _variables, _logger.Object).ToList();

        _executedScripts.Count.ShouldBe(1);
        _executedScripts[0].ShouldBe("select [some value]");

        actual.Count.ShouldBe(1);
        actual[0].ShouldBe(_executedReader.Object);

        _executedReader.VerifyAll();
    }

    [Test]
    public void GetDependencies()
    {
        _sut.ReadSqlContent = @"
-- module dependency: a 1.0
go
-- module dependency: b 1.0
go"
            .AsFuncStream();

        var actual = _sut.GetDependencies();

        actual.ShouldBe(new[] { new ScriptDependency("a", new Version("1.0")) });
    }
}