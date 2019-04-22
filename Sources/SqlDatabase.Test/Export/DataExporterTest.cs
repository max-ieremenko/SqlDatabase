using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Export
{
    [TestFixture]
    public class DataExporterTest
    {
        private StringBuilder _output;
        private DataExporter _exporter;

        [SetUp]
        public void BeforeEachTest()
        {
            _output = new StringBuilder();

            _exporter = new DataExporter
            {
                Output = new SqlWriter(new StringWriter(_output))
            };
        }

        [Test]
        [TestCaseSource(nameof(GetExportCases))]
        public void Export(string dataType, object minValue, object maxValue)
        {
            var sql = new StringBuilder();
            var script = new SqlWriter(new StringWriter(sql))
                .TextFormat("DECLARE @input TABLE(Value {0} NULL)", dataType)
                .Line()
                .Line("INSERT INTO @input VALUES");

            script.Text("(").Value(minValue).Line(")");
            script.Text(",(").Value(maxValue).Line(")");
            script.Text(",(").Value(null).Line(")");

            script.Text("SELECT * FROM @input");

            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql.ToString();
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    _exporter.Export(reader, "#tmp");
                }
            }

            var exportSql = _output.ToString();
            Console.WriteLine(exportSql);

            exportSql.ShouldContain(" " + dataType + " ");

            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = exportSql.Replace("GO", string.Empty) + "\r\n\r\nSELECT * FROM #tmp";
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read().ShouldBeTrue();
                    reader[0].ShouldBe(minValue);

                    reader.Read().ShouldBeTrue();
                    reader[0].ShouldBe(maxValue);

                    reader.Read().ShouldBeTrue();
                    reader[0].ShouldBe(DBNull.Value);
                }
            }
        }

        [Test]
        public void ExportReplaceRowVersionWithVarbinary()
        {
            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
declare @x table (Id int, Ver rowversion)
insert into @x(Id) values(1)
select * from @x";
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var table = _exporter.ReadSchemaTable(reader.GetSchemaTable(), "#tmp");
                    table.Columns[1].SqlDataTypeName.ShouldBe("VARBINARY");
                    table.Columns[1].Size.ShouldBe(8);

                    _exporter.Export(reader, "#tmp");
                }
            }

            var exportSql = _output.ToString();
            Console.WriteLine(exportSql);

            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = exportSql.Replace("GO", string.Empty) + "\r\n\r\nSELECT * FROM #tmp";
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read().ShouldBeTrue();
                    reader.Read().ShouldBeFalse();
                }
            }
        }

        [Test]
        public void ExportHierarchyId()
        {
            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
declare @x table (Id HIERARCHYID)
insert into @x values('/1/')
select * from @x";
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    Assert.Throws<NotSupportedException>(() => _exporter.ReadSchemaTable(reader.GetSchemaTable(), "#tmp"));
                }
            }
        }

        [Test]
        public void EmptySchemaTable()
        {
            ExportTable actual;

            using (var connection = new SqlConnection(Query.ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select 1, N'2' x, NULL";
                connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    actual = _exporter.ReadSchemaTable(reader.GetSchemaTable(), "#tmp");
                }
            }

            actual.Columns.Count.ShouldBe(3);

            actual.Columns[0].Name.ShouldBe("GeneratedName1");
            actual.Columns[0].AllowNull.ShouldBeFalse();
            actual.Columns[0].SqlDataTypeName.ShouldBe("int");

            actual.Columns[1].Name.ShouldBe("x");
            actual.Columns[1].AllowNull.ShouldBeFalse();
            actual.Columns[1].SqlDataTypeName.ShouldBe("nvarchar");

            actual.Columns[2].Name.ShouldBe("GeneratedName2");
            actual.Columns[2].AllowNull.ShouldBeTrue();
            actual.Columns[2].SqlDataTypeName.ShouldBe("int");
        }

        private static IEnumerable<TestCaseData> GetExportCases()
        {
            yield return new TestCaseData("BIGINT", long.MinValue, long.MaxValue) { TestName = "BIGINT" };
            yield return new TestCaseData("BIT", false, true) { TestName = "BIT" };
            yield return new TestCaseData("INT", int.MinValue, int.MaxValue) { TestName = "INT" };
            yield return new TestCaseData("SMALLINT", short.MinValue, short.MaxValue) { TestName = "SMALLINT" };
            yield return new TestCaseData("TINYINT", byte.MinValue, byte.MaxValue) { TestName = "TINYINT" };

            // NUMERIC is synonym
            yield return new TestCaseData("DECIMAL", -1.0, 1.0) { TestName = "DECIMAL" };
            yield return new TestCaseData("DECIMAL(38)", -1.0, 1.0) { TestName = "DECIMAL(38)" };
            yield return new TestCaseData("DECIMAL(38,3)", -1.123, 1.123) { TestName = "DECIMAL(38,3)" };
            yield return new TestCaseData("DECIMAL(18,1)", -1.1, 1.1) { TestName = "DECIMAL(18,1)" };
            yield return new TestCaseData("MONEY", -922_337_203_685_477.0, 922_337_203_685_477.0) { TestName = "MONEY" };
            yield return new TestCaseData("FLOAT", -1.0, 1.0) { TestName = "FLOAT" };

            yield return new TestCaseData("CHAR", "1", "2") { TestName = "CHAR" };
            yield return new TestCaseData("CHAR(3)", "123", "321") { TestName = "CHAR(3)" };
            yield return new TestCaseData("NCHAR", "1", "2") { TestName = "NCHAR" };
            yield return new TestCaseData("NCHAR(3)", "123", "321") { TestName = "NCHAR(3)" };
            yield return new TestCaseData("VARCHAR", "1", "2") { TestName = "VARCHAR" };
            yield return new TestCaseData("VARCHAR(5)", "123", "321") { TestName = "VARCHAR(5)" };
            yield return new TestCaseData("VARCHAR(MAX)", "123", "321") { TestName = "VARCHAR(MAX)" };
            yield return new TestCaseData("NVARCHAR", "1", "1") { TestName = "NVARCHAR" };
            yield return new TestCaseData("NVARCHAR(5)", "123", "321") { TestName = "NVARCHAR(5)" };
            yield return new TestCaseData("NVARCHAR(MAX)", "123", "321") { TestName = "NVARCHAR(MAX)" };
            yield return new TestCaseData("TEXT", "123", "321") { TestName = "TEXT" };
            yield return new TestCaseData("NTEXT", "123", "321") { TestName = "NTEXT" };
            yield return new TestCaseData("XML", "<xml />", "<data />") { TestName = "XML" };

            yield return new TestCaseData("UNIQUEIDENTIFIER", Guid.Empty, Guid.Empty) { TestName = "UNIQUEIDENTIFIER" };

            yield return new TestCaseData("BINARY", new[] { byte.MinValue }, new[] { byte.MaxValue }) { TestName = "BINARY" };
            yield return new TestCaseData("BINARY(2)", new[] { byte.MinValue, byte.MinValue }, new[] { byte.MaxValue, byte.MaxValue }) { TestName = "BINARY(2)" };

            yield return new TestCaseData("VARBINARY", new byte[0], new[] { byte.MinValue }) { TestName = "VARBINARY" };
            yield return new TestCaseData("VARBINARY(3)", new byte[0], new byte[] { byte.MinValue, 2, byte.MaxValue }) { TestName = "VARBINARY(3)" };
            yield return new TestCaseData("VARBINARY(MAX)", new byte[0], new byte[] { byte.MinValue, 2, byte.MaxValue }) { TestName = "VARBINARY(MAX)" };

            yield return new TestCaseData("IMAGE", new byte[0], new byte[] { byte.MinValue, 2, byte.MaxValue }) { TestName = "IMAGE" };

            var date = new DateTime(2019, 04, 22, 15, 42, 30);
            yield return new TestCaseData("DATE", date.Date, date.Date) { TestName = "DATE" };
            yield return new TestCaseData("DATETIME", date, date) { TestName = "DATETIME" };
            yield return new TestCaseData("DATETIME2", date, date) { TestName = "DATETIME2" };
            yield return new TestCaseData("SMALLDATETIME", date.AddSeconds(-30), date.AddSeconds(30)) { TestName = "SMALLDATETIME" };
            yield return new TestCaseData("TIME", date.TimeOfDay, date.TimeOfDay) { TestName = "TIME" };
            ////yield return new TestCaseData() { TestName = "" };
        }
    }
}
