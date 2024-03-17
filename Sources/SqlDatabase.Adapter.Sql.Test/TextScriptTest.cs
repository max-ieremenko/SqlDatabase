using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.Sql;

[TestFixture]
public class TextScriptTest
{
    private Mock<ILogger> _logger = null!;
    private Mock<IVariables> _variables = null!;
    private Mock<ISqlTextReader> _textReader = null!;
    private Mock<IDbCommand> _command = null!;
    private TextScript _sut = null!;

    private IList<string> _logOutput = null!;
    private IList<string> _executedScripts = null!;
    private Mock<IDataReader> _executedReader = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);
        _variables
            .Setup(v => v.GetValue("var1"))
            .Returns("[some value]");

        _textReader = new Mock<ISqlTextReader>(MockBehavior.Strict);

        _logOutput = new List<string>();
        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Info: {0}", m);
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

        _sut = new TextScript(null!, null!, _textReader.Object);
    }

    [Test]
    public void ExecuteShowVariableReplacement()
    {
        var sqlContent = new MemoryStream();
        _sut.ReadSqlContent = () => sqlContent;
        _textReader
            .Setup(r => r.ReadBatches(sqlContent))
            .Returns(new[] { "{{var1}} {{var1}}" });

        _executedReader
            .Setup(r => r.GetSchemaTable())
            .Returns((DataTable)null!);
        _executedReader
            .Setup(r => r.Read())
            .Returns(false);
        _executedReader
            .Setup(r => r.NextResult())
            .Returns(false);

        _sut.Execute(_command.Object, _variables.Object, _logger.Object);

        _executedScripts.Count.ShouldBe(1);
        _executedScripts[0].ShouldBe("[some value] [some value]");

        _logOutput.FirstOrDefault(i => i.Contains("var1") && i.Contains("[some value]")).ShouldNotBeNull();

        _executedReader.VerifyAll();
    }

    [Test]
    public void Execute()
    {
        var sqlContent = new MemoryStream();
        _sut.ReadSqlContent = () => sqlContent;
        _textReader
            .Setup(r => r.ReadBatches(sqlContent))
            .Returns(new[] { "{{var1}}", "text2" });

        _executedReader
            .Setup(r => r.GetSchemaTable())
            .Returns((DataTable)null!);
        _executedReader
            .Setup(r => r.Read())
            .Returns(false);
        _executedReader
            .Setup(r => r.NextResult())
            .Returns(false);

        _sut.Execute(_command.Object, _variables.Object, _logger.Object);

        _executedScripts.Count.ShouldBe(2);
        _executedScripts[0].ShouldBe("[some value]");
        _executedScripts[1].ShouldBe("text2");

        _executedReader.VerifyAll();
    }

    [Test]
    public void ExecuteWhatIf()
    {
        var sqlContent = new MemoryStream();
        _sut.ReadSqlContent = () => sqlContent;
        _textReader
            .Setup(r => r.ReadBatches(sqlContent))
            .Returns(new[] { "{{var1}}", "text2" });

        _sut.Execute(null, _variables.Object, _logger.Object);

        _executedScripts.ShouldBeEmpty();
        _textReader.VerifyAll();
    }

    [Test]
    public void ExecuteReader()
    {
        var sqlContent = new MemoryStream();
        _sut.ReadSqlContent = () => sqlContent;
        _textReader
            .Setup(r => r.ReadBatches(sqlContent))
            .Returns(new[] { "select {{var1}}" });

        var actual = _sut.ExecuteReader(_command.Object, _variables.Object, _logger.Object).ToList();

        _executedScripts.Count.ShouldBe(1);
        _executedScripts[0].ShouldBe("select [some value]");

        actual.Count.ShouldBe(1);
        actual[0].ShouldBe(_executedReader.Object);

        _executedReader.VerifyAll();
    }

    [Test]
    public void GetDependencies()
    {
        var sqlContent = new MemoryStream();
        _sut.ReadSqlContent = () => sqlContent;
        _textReader
            .Setup(r => r.ReadFirstBatch(sqlContent))
            .Returns("-- module dependency: a 1.0");

        var actual = _sut.GetDependencies();

        actual.ShouldNotBeNull();
        actual.ReadToEnd().ShouldBe("-- module dependency: a 1.0");
    }
}