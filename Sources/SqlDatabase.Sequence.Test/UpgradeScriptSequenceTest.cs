using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;
using SqlDatabase.TestApi;

namespace SqlDatabase.Sequence;

[TestFixture]
public class UpgradeScriptSequenceTest
{
    private UpgradeScriptSequence _sut = null!;
    private SourceFolder _root = null!;
    private Mock<IModuleVersionResolver> _versionResolver = null!;
    private Mock<IScriptFactory> _scriptFactory = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _root = new SourceFolder("Test");

        _scriptFactory = new Mock<IScriptFactory>(MockBehavior.Strict);
        _scriptFactory
            .Setup(f => f.IsSupported(It.IsAny<IFile>()))
            .Returns<IFile>(f => ".sql".Equals(f.Extension) || ".exe".Equals(f.Extension));

        _versionResolver = new Mock<IModuleVersionResolver>(MockBehavior.Strict);

        _sut = new UpgradeScriptSequence(
            _scriptFactory.Object,
            _versionResolver.Object,
            new IFileSystemInfo[] { _root },
            new Mock<ILogger>(MockBehavior.Strict).Object,
            false,
            false);
    }

    [Test]
    [TestCaseSource(nameof(GetBuildSequence))]
    public void BuildSequence(BuildSequenceCase testCase)
    {
        foreach (var sourceFile in testCase.Files)
        {
            var file = AddFile(_root, sourceFile.Name);

            var script = new Mock<IScript>(MockBehavior.Strict);
            script.SetupGet(s => s.DisplayName).Returns(file.Name);

            if (sourceFile.Dependencies == null)
            {
                script.Setup(s => s.GetDependencies()).Returns((TextReader?)null);
            }
            else
            {
                var dependenciesText = string.Join(
                    Environment.NewLine,
                    sourceFile.Dependencies.Select(i => $"-- module dependency: {i.Module} {i.Version}"));

                script.Setup(s => s.GetDependencies()).Returns(new StringReader(dependenciesText));
            }

            _scriptFactory
                .Setup(s => s.FromFile(file))
                .Returns(script.Object);
        }

        foreach (var version in testCase.Version)
        {
            _versionResolver
                .Setup(r => r.GetCurrentVersion(version.Module ?? string.Empty))
                .Returns(new Version(version.Version));
        }

        _sut.FolderAsModuleName = testCase.FolderAsModuleName;

        if (testCase.Exception == null)
        {
            var actual = _sut.BuildSequence();
            actual.Select(i => i.Script.DisplayName).ToArray().ShouldBe(testCase.Sequence);
        }
        else
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.BuildSequence());
            Console.WriteLine(ex!.Message);
            foreach (var tag in testCase.Exception)
            {
                ex.Message.ShouldContain(tag);
            }
        }
    }

    private static IEnumerable<TestCaseData> GetBuildSequence()
    {
        var anchor = typeof(UpgradeScriptSequenceTest);
        var prefix = anchor.Namespace + "." + anchor.Name + ".";

        var sources = anchor
            .Assembly
            .GetManifestResourceNames()
            .Where(i => i.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i);

        foreach (var sourceName in sources)
        {
            BuildSequenceCase[] testCases;
            using (var stream = anchor.Assembly.GetManifestResourceStream(sourceName))
            using (var reader = new JsonTextReader(new StreamReader(stream!)))
            {
                testCases = new JsonSerializer().Deserialize<BuildSequenceCase[]>(reader)!;
            }

            foreach (var testCase in testCases)
            {
                yield return new TestCaseData(testCase)
                {
                    TestName = sourceName.Substring(prefix.Length) + "-" + testCase.Name
                };
            }
        }
    }

    private static IFile AddFile(SourceFolder root, string fileName)
    {
        var path = fileName.Split('/');
        for (var i = 0; i < path.Length - 1; i++)
        {
            root = root.GetOrCreateSubFolder(path[i]);
        }

        return root.AddFile(path.Last());
    }

    public sealed class BuildSequenceCase
    {
        public string Name { get; set; } = null!;

        public bool FolderAsModuleName { get; set; }

        public ModuleVersion[] Version { get; set; } = null!;

        public SourceFile[] Files { get; set; } = null!;

        public string[] Sequence { get; set; } = null!;

        public string[]? Exception { get; set; }
    }

    public sealed class ModuleVersion
    {
        public string Module { get; set; } = null!;

        public string Version { get; set; } = null!;
    }

    public sealed class SourceFile
    {
        public string Name { get; set; } = null!;

        public ModuleVersion[]? Dependencies { get; set; }
    }

    private sealed class SourceFolder : IFolder
    {
        private readonly IDictionary<string, SourceFolder> _subFolderByName;
        private readonly IDictionary<string, IFile> _fileByName;

        public SourceFolder(string name)
        {
            Name = name;

            _subFolderByName = new Dictionary<string, SourceFolder>(StringComparer.OrdinalIgnoreCase);
            _fileByName = new Dictionary<string, IFile>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }

        public IEnumerable<IFolder> GetFolders() => _subFolderByName.Values;

        public IEnumerable<IFile> GetFiles() => _fileByName.Values;

        public SourceFolder GetOrCreateSubFolder(string name)
        {
            if (!_subFolderByName.TryGetValue(name, out var result))
            {
                result = new SourceFolder(name);
                _subFolderByName.Add(name, result);
            }

            return result;
        }

        public IFile AddFile(string name)
        {
            if (!_fileByName.TryGetValue(name, out var result))
            {
                result = FileFactory.File(name);
                _fileByName.Add(name, result);
            }

            return result;
        }
    }
}