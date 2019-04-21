using System;
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

        // TODO: Date and Time
        [Test]
        [TestCase("BIGINT", long.MinValue, long.MaxValue)]
        [TestCase("BIT", false, true)]
        [TestCase("INT", int.MinValue, int.MaxValue)]
        [TestCase("SMALLINT", short.MinValue, short.MaxValue)]
        [TestCase("TINYINT", byte.MinValue, byte.MaxValue)]
        [TestCase("DECIMAL", -1.0, 1.0)] // NUMERIC is synonym
        [TestCase("DECIMAL(38)", -1.0, 1.0)]
        [TestCase("DECIMAL(38,3)", -1.123, 1.123)]
        [TestCase("DECIMAL(18,1)", -1.1, 1.1)]
        [TestCase("MONEY", -922_337_203_685_477.0, 922_337_203_685_477.0)]
        [TestCase("FLOAT", -1.0, 1.0)]
        public void ExportNumeric(string dataType, object minValue, object maxValue)
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

        // TODO: escape
        [Test]
        [TestCase("CHAR", "1")]
        [TestCase("CHAR(3)", "123")]
        [TestCase("NCHAR", "1")]
        [TestCase("NCHAR(3)", "123")]
        [TestCase("VARCHAR", "1")]
        [TestCase("VARCHAR(5)", "123")]
        [TestCase("VARCHAR(MAX)", "123")]
        [TestCase("NVARCHAR", "1")]
        [TestCase("NVARCHAR(5)", "123")]
        [TestCase("NVARCHAR(MAX)", "123")]
        [TestCase("TEXT", "123")]
        [TestCase("NTEXT", "123")]
        [TestCase("XML", "<xml />")]
        public void ExportCharacter(string dataType, string value)
        {
            var sql = new StringBuilder();
            var script = new SqlWriter(new StringWriter(sql))
                .TextFormat("DECLARE @input TABLE(Value {0} NULL)", dataType)
                .Line()
                .Line("INSERT INTO @input VALUES");

            script.Text("(").Value(value).Line(")");
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
                    reader[0].ShouldBe(value);

                    reader.Read().ShouldBeTrue();
                    reader[0].ShouldBe(DBNull.Value);
                }
            }
        }

        [Test]
        public void EmptySchemaTable()
        {
        }
    }
}
