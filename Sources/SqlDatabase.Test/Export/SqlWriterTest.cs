using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;

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
        [TestCase("[schema].[Name]", "schema", "Name")]
        [TestCase("[schema].[Name]", "[schema]", "[Name]")]
        [TestCase("[schema].[Name]", "[schema", "Name]")]
        public void Name(string expected, params string[] names)
        {
            _sut.Name(names);

            _output.ToString().ShouldBe(expected);
        }
    }
}
