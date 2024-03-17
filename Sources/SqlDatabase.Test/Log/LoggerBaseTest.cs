using Moq;
using Moq.Protected;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Log;

[TestFixture]
public class LoggerBaseTest
{
    private IList<string> _info = null!;
    private IList<string> _error = null!;
    private LoggerBase _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _info = new List<string>();
        _error = new List<string>();

        var logger = new Mock<LoggerBase>(MockBehavior.Strict);
        logger
            .Protected()
            .Setup("WriteError", ItExpr.IsAny<string>())
            .Callback<string>(_error.Add);
        logger
            .Protected()
            .Setup("WriteInfo", ItExpr.IsAny<string>())
            .Callback<string>(_info.Add);

        _sut = logger.Object;
    }

    [Test]
    public void Info()
    {
        _sut.Info("the text");

        _info.ShouldBe(["the text"]);
        _error.ShouldBeEmpty();
    }

    [Test]
    public void Error()
    {
        _sut.Error("the text");

        _info.ShouldBeEmpty();
        _error.ShouldBe(["the text"]);
    }

    [Test]
    public void NoIndentOnError()
    {
        _sut.Indent();
        _sut.Error("the text");

        _error[0].ShouldBe("the text");
    }

    [Test]
    public void IndentInfo()
    {
        _sut.Info("1-");

        using (_sut.Indent())
        {
            _sut.Info("2-");

            using (_sut.Indent())
            {
                _sut.Info("3");
            }

            _sut.Info("2+");
        }

        _info.ShouldBe([
            "1-",
            "   2-",
            "      3",
            "   2+"
        ]);
    }
}