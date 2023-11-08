using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter.Sql.Export;

namespace SqlDatabase.Adapter.MySql;

[TestFixture]
public class MySqlDataExporterTest
{
    private StringBuilder _output = null!;
    private DataExporter _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new StringBuilder();

        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m => Console.WriteLine("Info: {0}", m));

        _sut = new DataExporter
        {
            Log = log.Object,
            Output = new MySqlWriter(new StringWriter(_output))
        };
    }

    [Test]
    [TestCaseSource(nameof(GetExportCases))]
    public void Export(string dataType, object minValue, object maxValue, bool allowNull, string expectedDataType)
    {
        var sql = new StringBuilder();
        var script = new MySqlWriter(new StringWriter(sql))
            .TextFormat("CREATE TEMPORARY TABLE input_data(value {0} {1});", dataType, allowNull ? "NULL" : null)
            .Line()
            .Line("INSERT INTO input_data VALUES");

        script.Text("(").Value(minValue, expectedDataType ?? dataType).Line(")");
        if (allowNull)
        {
            script.Text(",(").Value(maxValue, expectedDataType ?? dataType).Line(")");
            script.Text(",(").Value(null).Line(");");
        }
        else
        {
            script.Text(",(").Value(maxValue, expectedDataType ?? dataType).Line(");");
        }

        script.Text("SELECT * FROM input_data;");

        using (var connection = MySqlQuery.Open())
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sql.ToString();
            using (var reader = cmd.ExecuteReader())
            {
                _sut.Export(reader, "test_data");
            }
        }

        var exportSql = _output.ToString();
        Console.WriteLine(exportSql);

        exportSql.ShouldContain(" " + (expectedDataType ?? dataType) + " ");

        using (var connection = MySqlQuery.Open())
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = exportSql.Replace("CREATE TABLE", "CREATE TEMPORARY TABLE") + "\r\n\r\nSELECT * FROM test_data;";

            using (var reader = cmd.ExecuteReader())
            {
                reader.Read().ShouldBeTrue();
                CompareValues(expectedDataType ?? dataType, minValue, reader[0]);

                reader.Read().ShouldBeTrue();
                CompareValues(expectedDataType ?? dataType, maxValue, reader[0]);

                if (allowNull)
                {
                    reader.Read().ShouldBeTrue();
                    reader[0].ShouldBe(DBNull.Value);
                    reader.Read().ShouldBeFalse();
                }
                else
                {
                    reader.Read().ShouldBeFalse();
                }
            }
        }
    }

    private static void CompareValues(string dataType, object expected, object actual)
    {
        if (dataType == "TIMESTAMP")
        {
            actual = ((DateTime)actual) - new DateTime(1970, 01, 01);
        }

        if (dataType == "TIME")
        {
            var date = (DateTime)expected;
            expected = new TimeSpan(date.Hour, date.Minute, date.Second);
        }

        if (dataType == "GEOMETRY")
        {
            return;
        }

        actual.ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetExportCases()
    {
        // Integer Types
        yield return new TestCaseData("TINYINT", sbyte.MinValue, sbyte.MaxValue, true, "TINYINT(4)") { TestName = "TINYINT" };
        yield return new TestCaseData("TINYINT UNSIGNED", (sbyte)0, sbyte.MaxValue, true, "TINYINT(3) UNSIGNED") { TestName = "TINYINT UNSIGNED" };
        yield return new TestCaseData("SMALLINT", short.MinValue, short.MaxValue, true, "SMALLINT(6)") { TestName = "SMALLINT" };
        yield return new TestCaseData("SMALLINT UNSIGNED", ushort.MinValue, ushort.MaxValue, true, "SMALLINT(5) UNSIGNED") { TestName = "SMALLINT UNSIGNED" };
        yield return new TestCaseData("MEDIUMINT", -8388608, 8388607, true, "MEDIUMINT(9)") { TestName = "MEDIUMINT" };
        yield return new TestCaseData("MEDIUMINT UNSIGNED", 0, 16777215, true, "MEDIUMINT(8) UNSIGNED") { TestName = "MEDIUMINT UNSIGNED" };
        yield return new TestCaseData("INT", int.MinValue, int.MaxValue, true, "INT(11)") { TestName = "INT" };
        yield return new TestCaseData("INT UNSIGNED", uint.MinValue, uint.MaxValue, true, "INT(10) UNSIGNED") { TestName = "INT UNSIGNED" };
        yield return new TestCaseData("INTEGER", int.MinValue, int.MaxValue, true, "INT(11)") { TestName = "INTEGER" };
        yield return new TestCaseData("INTEGER UNSIGNED", uint.MinValue, uint.MaxValue, true, "INT(10) UNSIGNED") { TestName = "INTEGER UNSIGNED" };
        yield return new TestCaseData("BIGINT", long.MinValue, long.MaxValue, true, "BIGINT(20)") { TestName = "BIGINT" };
        yield return new TestCaseData("BIGINT UNSIGNED", ulong.MinValue, ulong.MaxValue, true, "BIGINT(20) UNSIGNED") { TestName = "BIGINT UNSIGNED" };

        // Fixed-Point Types
        yield return new TestCaseData("NUMERIC", -123M, 123M, true, "NUMERIC(10)") { TestName = "NUMERIC" };
        yield return new TestCaseData("DECIMAL", -123M, 123M, true, "NUMERIC(10)") { TestName = "DECIMAL" };
        yield return new TestCaseData("NUMERIC(6,3)", -123.123M, 123.123M, true, null) { TestName = "NUMERIC(6,3)" };

        // Floating-Point Types
        yield return new TestCaseData("FLOAT", -123f, 123f, true, null) { TestName = "FLOAT" };
        yield return new TestCaseData("FLOAT(7,4)", -123.21f, 123.21f, true, "FLOAT") { TestName = "FLOAT(7,4)" };
        yield return new TestCaseData("DOUBLE", -123f, 123f, true, null) { TestName = "DOUBLE" };
        yield return new TestCaseData("REAL", -123f, 123f, true, "DOUBLE") { TestName = "REAL" };
        yield return new TestCaseData("DOUBLE(7,4)", -123f, 123f, true, "DOUBLE") { TestName = "DOUBLE(7,4)" };

        // Bit-Value
        yield return new TestCaseData("BIT", 0, 1, true, "BIT(1)") { TestName = "BIT" };
        yield return new TestCaseData("BIT(10)", 7, 128, true, null) { TestName = "BIT(10)" };

        // bool
        yield return new TestCaseData("BOOL", true, false, true, null) { TestName = "BOOL" };
        yield return new TestCaseData("BOOLEAN", true, false, true, "BOOL") { TestName = "BOOLEAN" };

        // Date and Time Data Type
        yield return new TestCaseData("DATE", new DateTime(2021, 06, 20), new DateTime(2021, 06, 20), true, null) { TestName = "DATE" };
        yield return new TestCaseData("TIME", new DateTime(2021, 06, 20, 15, 10, 10), new DateTime(2021, 06, 20, 15, 10, 10), true, null) { TestName = "TIME" };
        yield return new TestCaseData("DATETIME", new DateTime(2021, 06, 20, 15, 10, 10), new DateTime(2021, 06, 20, 15, 10, 10), true, null) { TestName = "DATETIME" };
        yield return new TestCaseData("TIMESTAMP", TimeSpan.FromSeconds(1), TimeSpan.FromDays(2), true, null) { TestName = "TIMESTAMP" };
        yield return new TestCaseData("YEAR", 1901, 2155, true, null) { TestName = "YEAR" };

        // String Data Type
        yield return new TestCaseData("CHAR", "a", "b", true, "NCHAR(1)") { TestName = "CHAR" };
        yield return new TestCaseData("CHAR(10)", "ab", "bc", true, "NCHAR(10)") { TestName = "CHAR(10)" };
        yield return new TestCaseData("NCHAR(10)", "ab", "bc", true, null) { TestName = "NCHAR(10)" };
        yield return new TestCaseData("VARCHAR(10)", "ab", "bc", true, "NVARCHAR(10)") { TestName = "VARCHAR(10)" };
        yield return new TestCaseData("NVARCHAR(10)", "ab", "bc", true, null) { TestName = "NVARCHAR(10)" };
        yield return new TestCaseData("BINARY", new byte[] { 2 }, new byte[] { 1 }, true, "BINARY(1)") { TestName = "BINARY" };
        yield return new TestCaseData("VARBINARY(10)", new byte[] { 1, 2, 3 }, new byte[] { 1 }, true, null) { TestName = "VARBINARY(10)" };
        yield return new TestCaseData("TINYBLOB", new byte[] { 2 }, new byte[] { 1 }, true, "BLOB") { TestName = "TINYBLOB" };
        yield return new TestCaseData("TINYTEXT", "ab", "bc", true, "TEXT") { TestName = "TINYTEXT" };
        yield return new TestCaseData("BLOB", new byte[] { 2 }, new byte[] { 1 }, true, null) { TestName = "BLOB" };
        yield return new TestCaseData("TEXT", "ab", "bc", true, null) { TestName = "TEXT" };
        yield return new TestCaseData("MEDIUMBLOB", new byte[] { 2 }, new byte[] { 1 }, true, "BLOB") { TestName = "MEDIUMBLOB" };
        yield return new TestCaseData("MEDIUMTEXT", "ab", "bc", true, "TEXT") { TestName = "MEDIUMTEXT" };
        yield return new TestCaseData("LONGBLOB", new byte[] { 2 }, new byte[] { 1 }, true, "BLOB") { TestName = "LONGBLOB" };
        yield return new TestCaseData("LONGTEXT", "ab", "bc", true, "TEXT") { TestName = "LONGTEXT" };

        // enum
        yield return new TestCaseData("ENUM('small', 'medium', 'large')", "small", "large", true, "NVARCHAR(6)") { TestName = "ENUM" };
        yield return new TestCaseData("SET('a', 'b', 'c')", "a,b", "b,c", true, "NVARCHAR(5)") { TestName = "SET" };

        yield return new TestCaseData("JSON", "{}", "{\"a\": 1}", true, null) { TestName = "JSON" };

        yield return new TestCaseData("GEOMETRY", "ST_GeomFromText('POINT(1 1)')", "ST_GeomFromText('LINESTRING(0 0,1 1,2 2)')", true, null) { TestName = "GEOMETRY" };
        yield return new TestCaseData("POINT", "ST_GeomFromText('POINT(1 1)')", "ST_GeomFromText('POINT(1 1)')", true, "GEOMETRY") { TestName = "POINT" };
    }
}