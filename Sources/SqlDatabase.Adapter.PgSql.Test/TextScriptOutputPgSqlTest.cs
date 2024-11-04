using Moq;
using Npgsql;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter.Sql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PgSql;

[TestFixture]
public class TextScriptOutputPgSqlTest
{
    private NpgsqlConnection _connection = null!;
    private NpgsqlCommand _command = null!;
    private Mock<ILogger> _logger = null!;
    private Mock<IVariables> _variables = null!;

    private IList<string> _logOutput = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);

        _logOutput = new List<string>();
        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger
            .Setup(l => l.Indent())
            .Returns((IDisposable)null!);
        _logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _connection = PgSqlQuery.Open();
        _command = _connection.CreateCommand();
    }

    [TearDown]
    public void AfterEachTest()
    {
        _command?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public void ExecuteEmpty()
    {
        var sut = CreateSut("/* do nothing */");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.ShouldBeEmpty();
    }

    [Test]
    public void ExecuteDdlWithReader()
    {
        var sut = CreateSut(@"
create table public.TextScriptIntegrationTest(id int, name varchar(20));

insert into public.TextScriptIntegrationTest values(1, 'name 1');
insert into public.TextScriptIntegrationTest values(2, 'name 2');

select * from public.TextScriptIntegrationTest;

drop table public.TextScriptIntegrationTest;");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.Count.ShouldBe(8);
        _logOutput[0].ShouldBe("output: id; name");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("id   : 1");
        _logOutput[3].ShouldBe("name : name 1");
        _logOutput[4].ShouldBe("row 2");
        _logOutput[5].ShouldBe("id   : 2");
        _logOutput[6].ShouldBe("name : name 2");
        _logOutput[7].ShouldBe("2 rows selected");
    }

    [Test]
    public void NoColumnName()
    {
        var sut = CreateSut("select 1, 2");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.Count.ShouldBe(5);
        _logOutput[0].ShouldBe("output: ?column?; ?column?");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("?column? : 1");
        _logOutput[3].ShouldBe("?column? : 2");
        _logOutput[4].ShouldBe("1 row selected");
    }

    [Test]
    public void SelectNull()
    {
        var sut = CreateSut("select null");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.Count.ShouldBe(4);
        _logOutput[0].ShouldBe("output: ?column?");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("?column? : NULL");
        _logOutput[3].ShouldBe("1 row selected");
    }

    [Test]
    public void TwoSelections()
    {
        var sut = CreateSut(@"
select 1 first_;
select 2 second_;");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.Count.ShouldBe(9);

        _logOutput[0].ShouldBe("output: first_");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("first_ : 1");
        _logOutput[3].ShouldBe("1 row selected");

        _logOutput[4].ShouldBe(string.Empty);

        _logOutput[5].ShouldBe("output: second_");
        _logOutput[6].ShouldBe("row 1");
        _logOutput[7].ShouldBe("second_ : 2");
        _logOutput[8].ShouldBe("1 row selected");
    }

    [Test]
    public void SelectZeroRowsNull()
    {
        var sut = CreateSut("select null value_ limit 0");

        sut.Execute(_command, _variables.Object, _logger.Object);

        _logOutput.Count.ShouldBe(2);
        _logOutput[0].ShouldBe("output: value_");
        _logOutput[1].ShouldBe("0 rows selected");
    }

    private IScript CreateSut(string sql)
    {
        var file = FileFactory.File("dummy.sql", sql);
        return new TextScriptFactory(new PgSqlTextReader()).FromFile(file);
    }
}