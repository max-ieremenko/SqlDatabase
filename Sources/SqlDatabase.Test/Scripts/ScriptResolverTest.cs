using System;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts;

[TestFixture]
public class ScriptResolverTest
{
    private ScriptResolver _sut = null!;
    private Mock<IScriptFactory> _factory = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _factory = new Mock<IScriptFactory>(MockBehavior.Strict);
        _sut = new ScriptResolver(new[] { _factory.Object });
    }

    ////[Test]
    ////public void FromSqlFile()
    ////{
    ////    _textReader = new Mock<ISqlTextReader>(MockBehavior.Strict);

    ////    var file = FileFactory.File("11.sql", "some script");

    ////    _sut.IsSupported(file).ShouldBeTrue();

    ////    var script = _sut.FromFile(file).ShouldBeOfType<TextScript>();

    ////    script.DisplayName.ShouldBe("11.sql");
    ////    script.TextReader.ShouldBe(_textReader.Object);
    ////    new StreamReader(script.ReadSqlContent()).ReadToEnd().ShouldBe("some script");
    ////}

    [Test]
    public void FromFile()
    {
        var file = FileFactory.File(
            "11.ps1",
            "some script",
            FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

        _factory
            .Setup(f => f.IsSupported(file))
            .Returns(true);

        _sut.IsSupported(file).ShouldBeTrue();

        var script = new Mock<IScript>(MockBehavior.Strict);

        _factory
            .Setup(f => f.FromFile(file))
            .Returns(script.Object);

        _sut.FromFile(file).ShouldBe(script.Object);

        _factory.VerifyAll();
    }

    [Test]
    public void FromFileNotSupported()
    {
        var file = FileFactory.File("11.txt");

        _factory
            .Setup(f => f.IsSupported(file))
            .Returns(false);

        _sut.IsSupported(file).ShouldBeFalse();
        Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
    }
}