using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.MsSql;

[TestFixture]
public class MsSqlWriterTest
{
    private StringBuilder _output = null!;
    private MsSqlWriter _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new StringBuilder();
        _sut = new MsSqlWriter(new StringWriter(_output));
    }

    [TearDown]
    public void AfterEachTest()
    {
        TestOutput.WriteLine(_output);
    }

    [Test]
    public void Null()
    {
        _sut.Null();

        _output.ToString().ShouldBe("NULL");
    }

    [Test]
    [TestCase("[Name]", "Name")]
    [TestCase("[Name]", "[Name]")]
    public void Name(string expected, string value)
    {
        _sut.Name(value);

        _output.ToString().ShouldBe(expected);
    }

    [Test]
    [TestCase("abc")]
    [TestCase("'a'b''c'\r\na\rb\nc\ta")]
    [TestCase("/* comment */")]
    [TestCase("-- comment")]
    [TestCase("'")]
    public void EscapeVarchar(string input)
    {
        _sut.Text("SELECT ").Value(input);

        MsSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(input);
    }

    [Test]
    public void ValueDateTime()
    {
        var value = new DateTime(2019, 04, 21, 19, 26, 10).AddMilliseconds(123);

        _sut
            .Text("SELECT CAST(")
            .Value(value)
            .Text(" AS DATETIME2)");

        MsSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }

    [Test]
    public void ValueGuid()
    {
        var value = Guid.NewGuid();

        _sut
            .Text("SELECT CAST(")
            .Value(value)
            .Text(" AS UNIQUEIDENTIFIER)");

        MsSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }

    [Test]
    [TestCase(new byte[0])]
    [TestCase(new byte[] { 1, 2, 3 })]
    public void ValueByteArray(byte[] value)
    {
        _sut
            .Text("SELECT ")
            .Value(value);

        MsSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }

    [Test]
    public void ValueDateTimeOffset()
    {
        var value = new DateTimeOffset(2020, 11, 23, 00, 02, 50, 999, TimeSpan.FromHours(2));

        _sut
            .Text("SELECT CAST(")
            .Value(value)
            .Text(" AS DATETIMEOFFSET)");

        MsSqlQuery.ExecuteScalar(_output.ToString()).ShouldBe(value);
    }
}