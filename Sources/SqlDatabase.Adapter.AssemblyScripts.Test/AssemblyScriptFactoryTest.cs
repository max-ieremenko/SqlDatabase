using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.FileSystem;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class AssemblyScriptFactoryTest
{
    private AssemblyScriptFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new AssemblyScriptFactory("class-name", "method-name");
    }

    [Test]
    [TestCase(".EXE", true)]
    [TestCase(".dll", true)]
    [TestCase(".sql", false)]
    [TestCase(null, false)]
    public void IsSupported(string? ext, bool expected)
    {
        var file = new Mock<IFile>(MockBehavior.Strict);
        file
            .SetupGet(f => f.Extension)
            .Returns(ext!);

        _sut.IsSupported(file.Object).ShouldBe(expected);
    }

    [Test]
    public void FromFile()
    {
        var file = FileFactory.File(
            "11.dll",
            new byte[] { 1, 2, 3 },
            FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

        var script = _sut.FromFile(file).ShouldBeOfType<AssemblyScript>();

        script.DisplayName.ShouldBe("11.dll");
        script.ClassName.ShouldBe("class-name");
        script.MethodName.ShouldBe("method-name");
        script.ReadAssemblyContent().ShouldBe(new byte[] { 1, 2, 3 });
        new StreamReader(script.ReadDescriptionContent()!).ReadToEnd().ShouldBe("3, 2, 1");
    }
}