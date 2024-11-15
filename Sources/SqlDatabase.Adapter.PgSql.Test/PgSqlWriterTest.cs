﻿using System.Collections;
using NpgsqlTypes;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter.PgSql.UnmappedTypes;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PgSql;

[TestFixture]
public class PgSqlWriterTest
{
    private StringBuilder _output = null!;
    private PgSqlWriter _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new StringBuilder();
        _sut = new PgSqlWriter(new StringWriter(_output));
    }

    [TearDown]
    public void AfterEachTest()
    {
        TestOutput.WriteLine(_output);
    }

    [Test]
    public void ValueDateTime()
    {
        var value = new DateTime(2021, 05, 13, 18, 31, 30, 10);

        _sut
            .Text("SELECT ")
            .Value(value)
            .Text("::timestamp");

        PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }

    [Test]
    [TestCase("3.04:05:06")]
    [TestCase("04:05:06")]
    [TestCase("04:05:06.10")]
    public void ValueTimeSpan(string value)
    {
        var expected = TimeSpan.Parse(value);

        _sut
            .Text("SELECT ")
            .Value(expected)
            .Text("::interval");

        PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(expected);
    }

    [Test]
    [TestCase(new byte[0])]
    [TestCase(new byte[] { 1, 2, 3 })]
    public void ValueByteArray(byte[] value)
    {
        _sut
            .Text("SELECT ")
            .Value(value)
            .Text("::bytea");

        PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }

    [Test]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("f8955206-fa52-4cb3-b269-85ddd831d4d5")]
    public void ValueGuid(string value)
    {
        var expected = Guid.Parse(value);

        _sut
            .Text("SELECT ")
            .Value(expected)
            .Text("::uuid");

        PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(expected);
    }

    [Test]
    [TestCase(new byte[] { 1, 2, 3 })]
    [TestCase(new byte[] { 1 })]
    [TestCase(new byte[0])]
    public void ValueBitArray(byte[] values)
    {
        var expected = new BitArray(values);

        _sut
            .Text("SELECT ")
            .Value(expected);

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBeOfType<BitArray>();
        actual.Count.ShouldBe(expected.Count);

        for (var i = 0; i < actual.Count; i++)
        {
            actual[i].ShouldBe(expected[i]);
        }
    }

    [Test]
    [TestCase("a fat cat", "'a' 'cat' 'fat'")]
    [TestCase("fat & rat", "'&' 'fat' 'rat'")]
    public void ValueTsVector(string value, string expected)
    {
        _sut
            .Text("SELECT ")
            .Value(value)
            .Text("::tsvector");

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBeOfType<NpgsqlTsVector>();
        actual.ToString().ShouldBe(expected);
    }

    [Test]
    [TestCase("fat & rat")]
    [TestCase("fat & (rat | cat)")]
    public void ValueTsQuery(string value)
    {
        var query = PgSqlQuery.ExecuteScalar($"SELECT '{value}'::tsquery").ShouldBeAssignableTo<NpgsqlTsQuery>().ShouldNotBeNull();

        var expected = new StringBuilder();
        query.Write(expected);

        _sut
            .Text("SELECT ")
            .Value(query)
            .Text("::tsquery");

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBeAssignableTo<NpgsqlTsQuery>().ShouldNotBeNull();
        actual.ToString().ShouldBe(expected.ToString());
    }

    [Test]
    [TestCase("integer[]", 1, 2, 3)]
    [TestCase("text[]", "fat", "rat")]
    public void Value1dArray(string type, params object[] values)
    {
        _sut
            .Text("SELECT ")
            .Value(values, type)
            .Text("::" + type);

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString());
        actual.ShouldBe(values);
    }

    [Test]
    [TestCase("integer[][]", 1, 2, 3, 4)]
    [TestCase("text[][]", "fat", "rat", "cat", "flat")]
    public void Value2dArray(string type, object value11, object value12, object value21, object value22)
    {
        var array = Array.CreateInstance(value11.GetType(), 2, 2);
        array.SetValue(value11, 0, 0);
        array.SetValue(value12, 0, 1);
        array.SetValue(value21, 1, 0);
        array.SetValue(value22, 1, 1);

        _sut
            .Text("SELECT ")
            .Value(array, type)
            .Text("::" + type);

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString());
        actual.ShouldBe(array);
    }

    [Test]
    public void ValueCompositeType()
    {
        var expected = new Composite(["name", "supplier_id", "price"]);
        expected.Rows.Add(["fuzzy dice", 42, 1.99]);

        _sut
            .Text("SELECT ")
            .Value(expected)
            .Text("::public.inventory_item");

        Composite actual;
        using (var connection = PgSqlQuery.Open())
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = _output.ToString();
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read().ShouldBeTrue();
                actual = new PgSqlValueDataReader().Read(reader, 0).ShouldBeOfType<Composite>();
                reader.Read().ShouldBeFalse();
            }
        }

        actual.Names.ShouldBe(expected.Names);
        actual.Rows.Count.ShouldBe(1);
        actual.Rows[0].ShouldBe(expected.Rows[0]);
    }

    [Test]
    [TestCase("(20,30)", "[21,30)")]
    [TestCase("(20,30]", "[21,31)")]
    [TestCase("[20,30)", "[20,30)")]
    [TestCase("[20,30]", "[20,31)")]
    [TestCase("(,30)", "(,30)")]
    [TestCase("(20,)", "[21,)")]
    [TestCase("empty", "empty")]
    public void ValueRange(string value, string expected)
    {
        _sut
            .Text("SELECT ")
            .Value(NpgsqlRange<int>.Parse(value))
            .Text("::int4range");

        var actual = PgSqlQuery.ExecuteScalar(_output.ToString()).ShouldBeOfType<NpgsqlRange<int>>();
        actual.ShouldBeOneOf(NpgsqlRange<int>.Parse(value), NpgsqlRange<int>.Parse(expected));
    }
}