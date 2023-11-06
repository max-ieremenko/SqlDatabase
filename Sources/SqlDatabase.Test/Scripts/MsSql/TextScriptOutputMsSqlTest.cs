using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.MsSql;

[TestFixture]
public class TextScriptOutputMsSqlTest
{
    private SqlConnection _connection = null!;
    private SqlCommand _command = null!;
    private Mock<ILogger> _logger = null!;
    private Variables _variables = null!;
    private TextScript _sut = null!;

    private IList<string> _logOutput = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Variables();

        _logOutput = new List<string>();
        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger
            .Setup(l => l.Indent())
            .Returns((IDisposable)null!);
        _logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _sut = new TextScript("dummy", null!, new MsSqlTextReader());
        _connection = MsSqlQuery.Open();
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
        _sut.ReadSqlContent = "/* do nothing */".AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.ShouldBeEmpty();
    }

    [Test]
    public void ExecuteDdlWithReader()
    {
        _sut.ReadSqlContent = @"
create table dbo.TextScriptIntegrationTest(Id int, Name nvarchar(20))
go
insert into dbo.TextScriptIntegrationTest values(1, 'name 1')
insert into dbo.TextScriptIntegrationTest values(2, 'name 2')
go
select * from dbo.TextScriptIntegrationTest
go
drop table dbo.TextScriptIntegrationTest"
            .AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.Count.ShouldBe(8);
        _logOutput[0].ShouldBe("output: Id; Name");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("Id   : 1");
        _logOutput[3].ShouldBe("Name : name 1");
        _logOutput[4].ShouldBe("row 2");
        _logOutput[5].ShouldBe("Id   : 2");
        _logOutput[6].ShouldBe("Name : name 2");
        _logOutput[7].ShouldBe("2 rows selected");
    }

    [Test]
    public void NoColumnName()
    {
        _sut.ReadSqlContent = "select 1".AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.Count.ShouldBe(4);
        _logOutput[0].ShouldBe("output: (no name)");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("(no name) : 1");
        _logOutput[3].ShouldBe("1 row selected");
    }

    [Test]
    public void SelectNull()
    {
        _sut.ReadSqlContent = "select null".AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.Count.ShouldBe(4);
        _logOutput[0].ShouldBe("output: (no name)");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("(no name) : NULL");
        _logOutput[3].ShouldBe("1 row selected");
    }

    [Test]
    public void TwoSelections()
    {
        _sut.ReadSqlContent = @"
select 1 first
select 2 second"
            .AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.Count.ShouldBe(9);

        _logOutput[0].ShouldBe("output: first");
        _logOutput[1].ShouldBe("row 1");
        _logOutput[2].ShouldBe("first : 1");
        _logOutput[3].ShouldBe("1 row selected");

        _logOutput[4].ShouldBe(string.Empty);

        _logOutput[5].ShouldBe("output: second");
        _logOutput[6].ShouldBe("row 1");
        _logOutput[7].ShouldBe("second : 2");
        _logOutput[8].ShouldBe("1 row selected");
    }

    [Test]
    public void SelectZeroRowsNull()
    {
        _sut.ReadSqlContent = "select top 0 null value".AsFuncStream();

        _sut.Execute(_command, _variables, _logger.Object);

        _logOutput.Count.ShouldBe(2);
        _logOutput[0].ShouldBe("output: value");
        _logOutput[1].ShouldBe("0 rows selected");
    }
}