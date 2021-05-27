using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.PgSql
{
    [TestFixture]
    public class PgSqlTextReaderTest
    {
        private PgSqlTextReader _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new PgSqlTextReader();
        }

        [Test]
        public void ReadFirstBatch()
        {
            const string Expected = @"

/*
* module dependency: a 2.0
* module dependency: b 1.0
*/
;
line 2;";

            var actual = _sut.ReadFirstBatch(Expected.AsFuncStream()());

            actual.ShouldBe(@"/*
* module dependency: a 2.0
* module dependency: b 1.0
*/");
        }

        [Test]
        public void ReadBatches()
        {
            const string Expected = @"

line 1;
;
line 2;";

            var actual = _sut.ReadBatches(Expected.AsFuncStream()());

            actual.ShouldBe(new[] { Expected });
        }
    }
}
