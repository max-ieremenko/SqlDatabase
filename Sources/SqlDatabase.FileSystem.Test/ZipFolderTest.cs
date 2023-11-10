﻿using System.IO;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.FileSystem;

[TestFixture]
public class ZipFolderTest
{
    private TempDirectory _temp = null!;
    private ZipFolder _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _temp = new TempDirectory();
        _temp.CopyFileFromResources("Content.zip");
        _sut = new ZipFolder(Path.Combine(_temp.Location, "Content.zip"));
    }

    [TearDown]
    public void AfterEachTest()
    {
        _temp?.Dispose();
    }

    [Test]
    public void Ctor()
    {
        var folder = new ZipFolder(@"d:\11.zip");
        Assert.AreEqual("11.zip", folder.Name);
    }

    [Test]
    public void GetFolders()
    {
        var subFolders = _sut.GetFolders().OrderBy(i => i.Name).ToArray();

        CollectionAssert.AreEqual(
            new[] { "1", "2", "inner.zip" },
            subFolders.Select(i => i.Name).ToArray());
    }

    [Test]
    public void GetFiles()
    {
        var files = _sut.GetFiles().OrderBy(i => i.Name).ToArray();
        CollectionAssert.AreEqual(new[] { "11.txt" }, files.Select(i => i.Name).ToArray());

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            Assert.AreEqual("11", reader.ReadToEnd());
        }

        files[0].GetParent()!.GetFiles().OrderBy(i => i.Name).First().ShouldBe(files[0]);
    }

    [Test]
    public void GetContentOfSubFolders()
    {
        var subFolders = _sut.GetFolders().OrderBy(i => i.Name).ToArray();

        // 1
        Assert.AreEqual(0, subFolders[0].GetFolders().Count());
        Assert.AreEqual(0, subFolders[0].GetFiles().Count());

        // 2
        Assert.AreEqual(1, subFolders[1].GetFolders().Count());

        var files = subFolders[1].GetFiles().ToArray();
        CollectionAssert.AreEqual(new[] { "22.txt" }, files.Select(i => i.Name).ToArray());

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            Assert.AreEqual("22", reader.ReadToEnd());
        }

        files[0].GetParent().ShouldBe(subFolders[1]);
    }

    [Test]
    public void ReadContentOfEmbeddedZip()
    {
        var innerZip = _sut.GetFolders().OrderBy(i => i.Name).Last();

        Assert.AreEqual(2, innerZip.GetFolders().Count());

        var files = innerZip.GetFiles().ToArray();
        CollectionAssert.AreEqual(new[] { "11.txt" }, files.Select(i => i.Name).ToArray());

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            Assert.AreEqual("11", reader.ReadToEnd());
        }

        files[0].GetParent()!.GetFiles().OrderBy(i => i.Name).First().ShouldBe(files[0]);
    }
}