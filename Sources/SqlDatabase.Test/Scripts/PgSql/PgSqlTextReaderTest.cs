using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.PgSql
{
    [TestFixture]
    public class PgSqlTextReaderTest
    {
        [Test]
        public void Read()
        {
            const string Expected = "abc";

            var actual = new PgSqlTextReader().Read(Expected.AsFuncStream()());

            actual.ShouldBe(new[] { Expected });
        }
    }
}
