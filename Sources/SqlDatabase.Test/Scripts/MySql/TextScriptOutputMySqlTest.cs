using System;
using System.Collections.Generic;
using Moq;
using MySqlConnector;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.MySql
{
    [TestFixture]
    public class TextScriptOutputMySqlTest
    {
        private MySqlConnection _connection;
        private MySqlCommand _command;
        private Mock<ILogger> _logger;
        private Variables _variables;
        private TextScript _sut;

        private IList<string> _logOutput;

        [SetUp]
        public void BeforeEachTest()
        {
            _variables = new Variables();

            _logOutput = new List<string>();
            _logger = new Mock<ILogger>(MockBehavior.Strict);
            _logger
                .Setup(l => l.Indent())
                .Returns((IDisposable)null);
            _logger
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });

            _sut = new TextScript
            {
                TextReader = new MySqlTextReader()
            };

            _connection = MySqlQuery.Open();
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
create table TextScriptIntegrationTest(id int, name varchar(20));

insert into TextScriptIntegrationTest values(1, 'name 1');
insert into TextScriptIntegrationTest values(2, 'name 2');

select * from TextScriptIntegrationTest;

drop table TextScriptIntegrationTest;"
                .AsFuncStream();

            _sut.Execute(_command, _variables, _logger.Object);

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
            _sut.ReadSqlContent = "select 1".AsFuncStream();

            _sut.Execute(_command, _variables, _logger.Object);

            _logOutput.Count.ShouldBe(4);
            _logOutput[0].ShouldBe("output: 1");
            _logOutput[1].ShouldBe("row 1");
            _logOutput[2].ShouldBe("1 : 1");
            _logOutput[3].ShouldBe("1 row selected");
        }

        [Test]
        public void SelectNull()
        {
            _sut.ReadSqlContent = "select null".AsFuncStream();

            _sut.Execute(_command, _variables, _logger.Object);

            _logOutput.Count.ShouldBe(4);
            _logOutput[0].ShouldBe("output: NULL");
            _logOutput[1].ShouldBe("row 1");
            _logOutput[2].ShouldBe("NULL : NULL");
            _logOutput[3].ShouldBe("1 row selected");
        }

        [Test]
        public void TwoSelections()
        {
            _sut.ReadSqlContent = @"
select 1 first_;
select 2 second_;"
                .AsFuncStream();

            _sut.Execute(_command, _variables, _logger.Object);

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
            _sut.ReadSqlContent = "select null value_ limit 0".AsFuncStream();

            _sut.Execute(_command, _variables, _logger.Object);

            _logOutput.Count.ShouldBe(2);
            _logOutput[0].ShouldBe("output: value_");
            _logOutput[1].ShouldBe("0 rows selected");
        }
    }
}
