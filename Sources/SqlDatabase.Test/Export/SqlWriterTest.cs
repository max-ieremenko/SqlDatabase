using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Export
{
    [TestFixture]
    public class SqlWriterTest
    {
        private StringBuilder _output;
        private SqlWriter _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _output = new StringBuilder();
            _sut = new SqlWriter(new StringWriter(_output));
        }

        [Test]
        public void Null()
        {
            _sut.Null();

            _output.ToString().ShouldBe("NULL");
        }

        [Test]
        [TestCase("[Name]", "Name")]
        [TestCase("[Name]", "[Name]")]
        public void Name(string expected, string value)
        {
            _sut.Name(value);

            _output.ToString().ShouldBe(expected);
        }

        [Test]
        [TestCase("abc")]
        [TestCase("'a'b''c'\r\na\rb\nc\ta")]
        [TestCase("/* comment */")]
        [TestCase("-- comment")]
        [TestCase("'")]
        public void EscapeVarchar(string input)
        {
            _sut.Text("SELECT ").Value(input);

            Query.ExecuteScalar(_output.ToString()).ShouldBe(input);
        }

        [Test]
        public void ValueDateTime()
        {
            var value = new DateTime(2019, 04, 21, 19, 26, 10).AddMilliseconds(123);

            _sut
                .Text("SELECT CAST(")
                .Value(value)
                .Text(" AS DATETIME2)");

            Query.ExecuteScalar(_output.ToString()).ShouldBe(value);
        }

        [Test]
        public void ValueGuid()
        {
            var value = Guid.NewGuid();

            _sut
                .Text("SELECT CAST(")
                .Value(value)
                .Text(" AS UNIQUEIDENTIFIER)");

            Query.ExecuteScalar(_output.ToString()).ShouldBe(value);
        }

        [Test]
        public void ValueByteArray()
        {
            var value = new byte[0];

            _sut
                .Text("SELECT ")
                .Value(value);

            Query.ExecuteScalar(_output.ToString()).ShouldBe(value);
        }

        [Test]
        public void ValueEmptyByteArray()
        {
            var value = new byte[0];

            _sut
                .Text("SELECT ")
                .Value(value);

            Query.ExecuteScalar(_output.ToString()).ShouldBe(value);
        }
    }
}
