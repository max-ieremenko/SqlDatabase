using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using Moq;
using Npgsql;
using NpgsqlTypes;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Scripts.PgSql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Export;

[TestFixture]
public class PgSqlDataExporterTest
{
    private StringBuilder _output;
    private DataExporter _sut;

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
            Output = new PgSqlWriter(new StringWriter(_output))
        };
    }

    [Test]
    [TestCaseSource(nameof(GetExportCases))]
    public void Export(string dataType, object minValue, object maxValue, bool allowNull, string expectedDataType)
    {
        var sql = new StringBuilder();
        var script = new PgSqlWriter(new StringWriter(sql))
            .TextFormat("CREATE TEMP TABLE input_data(value {0} {1});", dataType, allowNull ? "NULL" : null)
            .Line()
            .Line("INSERT INTO input_data VALUES");

        script.Text("(").Value(minValue).Line(")");
        if (allowNull)
        {
            script.Text(",(").Value(maxValue).Line(")");
            script.Text(",(").Value(null).Line(");");
        }
        else
        {
            script.Text(",(").Value(maxValue).Line(");");
        }

        script.Text("SELECT * FROM input_data;");

        using (var connection = new NpgsqlConnection(PgSqlQuery.ConnectionString))
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sql.ToString();
            connection.Open();

            using (var reader = cmd.ExecuteReader())
            {
                _sut.Export(reader, "test_data");
            }
        }

        var exportSql = _output.ToString();
        Console.WriteLine(exportSql);

        exportSql.ShouldContain(" " + (expectedDataType ?? dataType) + " ");

        using (var connection = new NpgsqlConnection(PgSqlQuery.ConnectionString))
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = exportSql.Replace("CREATE TABLE", "CREATE TEMP TABLE") + "\r\n\r\nSELECT * FROM test_data;";
            connection.Open();

            using (var reader = cmd.ExecuteReader())
            {
                reader.Read().ShouldBeTrue();
                CompareValues(dataType, minValue, reader[0]);

                reader.Read().ShouldBeTrue();
                CompareValues(dataType, maxValue, reader[0]);

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
        if (dataType.StartsWith("bit", StringComparison.OrdinalIgnoreCase) && actual is bool)
        {
            actual.ShouldBe(((BitArray)expected)[0]);
            return;
        }

        if (dataType.Equals("tsvector", StringComparison.OrdinalIgnoreCase))
        {
            actual.ShouldBeOfType<NpgsqlTsVector>().ShouldBe(NpgsqlTsVector.Parse((string)expected));
            return;
        }

        if (dataType.Equals("tsquery", StringComparison.OrdinalIgnoreCase))
        {
            actual.ShouldBeAssignableTo<NpgsqlTsQuery>().ToString().ShouldBe(NpgsqlTsQuery.Parse((string)expected).ToString());
            return;
        }

        if (expected is IDictionary<string, object> compositeExpected)
        {
            var compositeActual = actual.ShouldBeAssignableTo<IDictionary<string, object>>();
            compositeActual.Keys.ShouldBe(compositeExpected.Keys);
            foreach (var key in compositeExpected.Keys)
            {
                compositeActual[key].ShouldBe(compositeExpected[key]);
            }

            return;
        }

        actual.ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetExportCases()
    {
        // Numeric Types
        yield return new TestCaseData("smallint", short.MinValue, short.MaxValue, true, null) { TestName = "smallint" };
        yield return new TestCaseData("integer", int.MinValue, int.MaxValue, true, null) { TestName = "integer" };
        yield return new TestCaseData("bigint", long.MinValue, long.MaxValue, true, null) { TestName = "bigint" };
        yield return new TestCaseData("decimal", decimal.MinValue, decimal.MaxValue, true, "numeric") { TestName = "decimal" };
        yield return new TestCaseData("numeric", decimal.MinValue, decimal.MaxValue, true, null) { TestName = "numeric" };
        yield return new TestCaseData("real", -10.1F, 20.1F, true, null) { TestName = "real" };
        yield return new TestCaseData("double precision", double.MinValue, double.MaxValue, true, null) { TestName = "double precision" };

        yield return new TestCaseData("smallserial", (short)0, short.MaxValue, false, "smallint") { TestName = "smallserial" };
        yield return new TestCaseData("serial", 0, int.MaxValue, false, "integer") { TestName = "serial" };
        yield return new TestCaseData("bigserial", 0L, long.MaxValue, false, "bigint") { TestName = "bigserial" };

        yield return new TestCaseData("numeric(2)", -1m, 1m, true, null) { TestName = "numeric(2)" };
        yield return new TestCaseData("numeric(2,1)", -1.1m, 1.1m, true, null) { TestName = "numeric(2,1)" };

        // Monetary Types
        yield return new TestCaseData("money", -100.12m, 100.12m, true, null) { TestName = "money" };

        // Character Types
        yield return new TestCaseData("character varying", "abc", "d", true, null) { TestName = "character varying" };
        yield return new TestCaseData("varchar", "abc", "d", true, "character varying") { TestName = "varchar" };
        yield return new TestCaseData("character varying(3)", "abc", "d", true, null) { TestName = "character varying(3)" };
        yield return new TestCaseData("varchar(3)", "abc", "d", true, "character varying(3)") { TestName = "varchar(3)" };

        yield return new TestCaseData("character", "a", "d", true, "character(1)") { TestName = "character" };
        yield return new TestCaseData("char", "a", "d", true, "character(1)") { TestName = "char" };
        yield return new TestCaseData("character(2)", "ab", "db", true, null) { TestName = "character(2)" };
        yield return new TestCaseData("char(2)", "ab", "db", true, "character(2)") { TestName = "char(2)" };

        yield return new TestCaseData("text", "abc", "d", true, null) { TestName = "text" };

        yield return new TestCaseData("name", "abc", "d", true, null) { TestName = "name" };

        yield return new TestCaseData("citext", "abc", "d", true, "public.citext") { TestName = "citext" };
        yield return new TestCaseData("public.citext", "abc", "d", true, null) { TestName = "public.citext" };

        // Binary Data Types
        yield return new TestCaseData("bytea", new byte[0], new[] { byte.MinValue, byte.MaxValue, (byte)10 }, true, null) { TestName = "bytea" };

        // Date/Time Types
        var date = new DateTime(2021, 05, 13, 18, 31, 30, 10);
        yield return new TestCaseData("timestamp", date, date, true, null) { TestName = "timestamp" };
        yield return new TestCaseData("timestamp(6)", date, date, true, null) { TestName = "timestamp(6)" };
        yield return new TestCaseData("date", date.Date, date.Date, true, null) { TestName = "date" };
        yield return new TestCaseData("time", date.TimeOfDay, date.TimeOfDay, true, null) { TestName = "time" };
        yield return new TestCaseData("time(6)", date.TimeOfDay, date.TimeOfDay, true, null) { TestName = "time(6)" };
        yield return new TestCaseData("interval", TimeSpan.Parse("3.04:05:06"), TimeSpan.Parse("04:05:06"), true, null) { TestName = "interval" };
        yield return new TestCaseData("interval(6)", TimeSpan.Parse("3.04:05:06"), TimeSpan.Parse("04:05:06"), true, null) { TestName = "interval(6)" };

        // Boolean Type
        yield return new TestCaseData("boolean", true, false, true, null) { TestName = "boolean" };

        // UUID Type
        yield return new TestCaseData("uuid", Guid.Empty, Guid.NewGuid(), true, null) { TestName = "uuid" };

        // Bit String Types
        yield return new TestCaseData("bit", new BitArray(new[] { true }), new BitArray(new[] { false }), true, "bit(1)") { TestName = "bit" };
        yield return new TestCaseData("bit(2)", new BitArray(new[] { false, true }), new BitArray(new[] { true, false }), true, null) { TestName = "bit(2)" };

        // XML Type
        yield return new TestCaseData("xml", "<foo>bar</foo>", "<foo>bar</foo>", true, null) { TestName = "xml" };

        // JSON Types
        yield return new TestCaseData("json", "{\"foo\": \"bar\"}", "{\"foo\": \"bar\"}", true, null) { TestName = "json" };
        yield return new TestCaseData("jsonb", "{\"foo\": \"bar\"}", "{\"foo\": \"bar\"}", true, null) { TestName = "jsonb" };

        // Text Search Types
        yield return new TestCaseData("tsvector", "a fat cat", "a fat cat", true, null) { TestName = "tsvector" };
        yield return new TestCaseData("tsquery", "fat & rat", "fat & rat", true, null) { TestName = "tsquery" };

        // Enumerated Types
        yield return new TestCaseData("public.mood", "ok", "happy", true, null) { TestName = "enum1" };
        yield return new TestCaseData("mood", "ok", "happy", true, "public.mood") { TestName = "enum2" };

        // Arrays
        yield return new TestCaseData("integer[]", new[] { 1, 2, 3 }, new[] { -1, -2, 3 }, true, null) { TestName = "integer[]" };
        yield return new TestCaseData("integer[3]", new[] { 1, 2, 3 }, new[] { -1, -2, 3 }, true, "integer[]") { TestName = "integer[3]" };

        // Composite Types
        IDictionary<string, object> composite = new ExpandoObject();
        composite.Add("name", "fuzzy dice");
        composite.Add("supplier_id", 42);
        composite.Add("price", 1.99);
        yield return new TestCaseData("public.inventory_item", composite, composite, true, null) { TestName = "composite" };

        // Range Types
        yield return new TestCaseData("int4range", NpgsqlRange<int>.Parse("[21,30)"), NpgsqlRange<int>.Parse("[21,31)"), true, null) { TestName = "int4range" };
    }
}