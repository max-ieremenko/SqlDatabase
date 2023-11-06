using System;
using System.Configuration;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Configuration;

[TestFixture]
public class CommandLineBaseTest
{
    private Mock<IFileSystemFactory> _fs = null!;
    private CommandLineBase _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

        _sut = new Mock<CommandLineBase> { CallBase = true }.Object;
        _sut.ConnectionString = "connection-string";
        _sut.FileSystemFactory = _fs.Object;
    }

    [Test]
    public void ParseFrom()
    {
        var file = new Mock<IFileSystemInfo>(MockBehavior.Strict);
        _fs
            .Setup(f => f.FileSystemInfoFromPath(@"c:\11.sql"))
            .Returns(file.Object);

        _sut.Parse(new CommandLine(new Arg("from", @"c:\11.sql")));

        _sut.Scripts.ShouldBe(new[] { file.Object });
    }
}