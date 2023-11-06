using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;

namespace SqlDatabase.Sequence;

[TestFixture]
public class ModuleVersionResolverTest
{
    private ModuleVersionResolver _sut = null!;
    private IList<string> _logOutput = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _logOutput = new List<string>();
        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _sut = new ModuleVersionResolver(log.Object, null!);
    }

    [Test]
    public void GetCurrentVersionModuleName()
    {
        const string ModuleName = "the-module";
        var moduleVersion = new Version("1.0");

        _sut.Database = name =>
        {
            name.ShouldBe(ModuleName);
            return moduleVersion;
        };

        _sut.GetCurrentVersion(ModuleName).ShouldBe(moduleVersion);

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldContain(ModuleName);
        _logOutput[0].ShouldContain(moduleVersion.ToString());
    }

    [Test]
    public void GetCurrentVersionNoModule()
    {
        var version = new Version("1.0");

        _sut.Database = name =>
        {
            name.ShouldBeEmpty();
            return version;
        };

        _sut.GetCurrentVersion(null).ShouldBe(version);

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldContain("database version");
        _logOutput[0].ShouldContain(version.ToString());
    }

    [Test]
    public void GetCurrentVersionCache()
    {
        var version = new Version("1.0");

        _sut.Database = name =>
        {
            name.ShouldBeEmpty();
            return version;
        };

        _sut.GetCurrentVersion(null).ShouldBe(version);

        _logOutput.Clear();
        _sut.Database = name =>
        {
            name.ShouldBeEmpty();
            throw new NotSupportedException();
        };

        _sut.GetCurrentVersion(null).ShouldBe(version);
        _logOutput.ShouldBeEmpty();
    }
}