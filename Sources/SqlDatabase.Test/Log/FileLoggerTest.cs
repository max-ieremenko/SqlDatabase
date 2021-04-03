using System;
using System.IO;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Log
{
    [TestFixture]
    public class FileLoggerTest
    {
        private TempFile _file;

        [SetUp]
        public void BeforeEachTest()
        {
            _file = new TempFile(".log");
        }

        [TearDown]
        public void AfterEachTest()
        {
            _file?.Dispose();
        }

        [Test]
        public void Info()
        {
            using (var sut = new FileLogger(_file.Location))
            {
                sut.Info("some message");
            }

            var actual = File.ReadAllText(_file.Location);

            Console.WriteLine(actual);
            actual.ShouldContain(" INFO ");
            actual.ShouldContain(" some message");
        }

        [Test]
        public void Error()
        {
            using (var sut = new FileLogger(_file.Location))
            {
                sut.Error("some message");
            }

            var actual = File.ReadAllText(_file.Location);

            Console.WriteLine(actual);
            actual.ShouldContain(" ERROR ");
            actual.ShouldContain(" some message");
        }

        [Test]
        public void Append()
        {
            File.WriteAllLines(_file.Location, new[] { "do not remove" });

            using (var sut = new FileLogger(_file.Location))
            {
                sut.Info("some message");
                sut.Error("some error");
            }

            var actual = File.ReadAllText(_file.Location);

            Console.WriteLine(actual);
            actual.ShouldContain("do not remove");
            actual.ShouldContain(" INFO ");
            actual.ShouldContain(" some message");
            actual.ShouldContain(" ERROR ");
            actual.ShouldContain(" some error");
        }

        [Test]
        public void FileIsAvailableForRead()
        {
            string actual;
            using (var sut = new FileLogger(_file.Location))
            {
                sut.Info("some message");
                sut.Flush();

                using (var stream = new FileStream(_file.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    actual = reader.ReadToEnd();
                }
            }

            Console.WriteLine(actual);
            actual.ShouldContain(" INFO ");
            actual.ShouldContain(" some message");
        }

        [Test]
        public void WriteNull()
        {
            using (var sut = new FileLogger(_file.Location))
            {
                sut.Info(null);
            }

            var actual = File.ReadAllText(_file.Location);

            Console.WriteLine(actual);
            actual.ShouldContain(" INFO ");
        }
    }
}
