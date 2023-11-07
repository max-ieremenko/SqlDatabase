﻿using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.MySql;
using SqlDatabase.Scripts.PgSql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts;

[TestFixture]
public class DatabaseAdapterFactoryTest
{
    private ILogger _log = null!;
    private AppConfiguration _configuration = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _log = new Mock<ILogger>(MockBehavior.Strict).Object;
        _configuration = new AppConfiguration();
    }

    [Test]
    [TestCaseSource(nameof(GetCreateAdapterCases))]
    public void CreateAdapter(string connectionString, string databaseName, string expected)
    {
        var actual = DatabaseAdapterFactory.CreateAdapter(connectionString, _configuration, _log);

        actual.GetType().Name.ShouldBe(expected);
        actual.DatabaseName.ShouldBe(databaseName);
    }

    private static IEnumerable<TestCaseData> GetCreateAdapterCases()
    {
        yield return new TestCaseData(
            "Data Source=.;Initial Catalog=SqlDatabaseTest",
            "SqlDatabaseTest",
            "MsSqlDatabaseAdapter")
        {
            TestName = "MsSql"
        };

        yield return new TestCaseData(
            PgSqlQuery.ConnectionString,
            PgSqlQuery.DatabaseName,
            nameof(PgSqlDatabaseAdapter))
        {
            TestName = "PgSql"
        };

        yield return new TestCaseData(
            MySqlQuery.ConnectionString,
            MySqlQuery.DatabaseName,
            nameof(MySqlDatabaseAdapter))
        {
            TestName = "MySql"
        };
    }
}