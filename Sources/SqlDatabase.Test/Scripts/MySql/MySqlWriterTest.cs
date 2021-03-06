﻿using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.MySql
{
    [TestFixture]
    public class MySqlWriterTest
    {
        private StringBuilder _output;
        private MySqlWriter _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _output = new StringBuilder();
            _sut = new MySqlWriter(new StringWriter(_output));
        }

        [TearDown]
        public void AfterEachTest()
        {
            Console.WriteLine(_output);
        }

        [Test]
        public void ValueDateTime()
        {
            var value = new DateTime(2021, 06, 19, 19, 42, 30, 0);

            _sut
                .Text("SELECT CAST(")
                .Value(value)
                .Text(" AS DATETIME)");

            MySqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
        }
    }
}
