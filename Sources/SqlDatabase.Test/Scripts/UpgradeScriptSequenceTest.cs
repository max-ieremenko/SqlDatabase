using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.IO;
using SqlDatabase.Scripts.UpgradeInternal;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class UpgradeScriptSequenceTest
    {
        private UpgradeScriptSequence _sut;
        private Mock<IFolder> _root;
        private Mock<IModuleVersionResolver> _versionResolver;
        private Mock<IScriptFactory> _scriptFactory;

        [SetUp]
        public void BeforeEachTest()
        {
            _root = new Mock<IFolder>(MockBehavior.Strict);

            _scriptFactory = new Mock<IScriptFactory>(MockBehavior.Strict);
            _scriptFactory
                .Setup(f => f.IsSupported(It.IsAny<string>()))
                .Returns<string>(s => ".sql".Equals(Path.GetExtension(s)) || ".exe".Equals(Path.GetExtension(s)));

            _versionResolver = new Mock<IModuleVersionResolver>(MockBehavior.Strict);

            _sut = new UpgradeScriptSequence
            {
                Sources = { _root.Object },
                ScriptFactory = _scriptFactory.Object,
                VersionResolver = _versionResolver.Object
            };
        }

        [Test]
        [TestCaseSource(nameof(GetBuildSequence))]
        public void BuildSequence(BuildSequenceCase testCase)
        {
            var folderByName = new Dictionary<string, List<IFile>>(StringComparer.OrdinalIgnoreCase);
            var files = new List<IFile>();

            foreach (var sourceFile in testCase.Files)
            {
                var folderName = Path.GetDirectoryName(sourceFile.Name);
                var folder = files;

                if (!string.IsNullOrEmpty(folderName))
                {
                    if (!folderByName.ContainsKey(folderName))
                    {
                        folderByName.Add(folderName, new List<IFile>());
                    }

                    folder = folderByName[folderName];
                }

                var file = FileFactory.File(Path.GetFileName(sourceFile.Name));
                folder.Add(file);

                var dependencies = new ScriptDependency[0];
                if (sourceFile.Dependencies != null)
                {
                    dependencies = sourceFile.Dependencies.Select(i => new ScriptDependency(i.Module, new Version(i.Version))).ToArray();
                }

                var script = new Mock<IScript>(MockBehavior.Strict);
                script.SetupGet(s => s.DisplayName).Returns(file.Name);
                script.Setup(s => s.GetDependencies()).Returns(dependencies);

                _scriptFactory
                    .Setup(s => s.FromFile(file))
                    .Returns(script.Object);
            }

            _root.Setup(r => r.GetFolders()).Returns(folderByName.Select(i => FileFactory.Folder(i.Key, i.Value.ToArray())));
            _root.Setup(r => r.GetFiles()).Returns(files);

            foreach (var version in testCase.Version)
            {
                _versionResolver
                    .Setup(r => r.GetCurrentVersion(version.Module ?? string.Empty))
                    .Returns(new Version(version.Version));
            }

            if (testCase.Exception == null)
            {
                var actual = _sut.BuildSequence();
                actual.Select(i => i.Script.DisplayName).ToArray().ShouldBe(testCase.Sequence);
            }
            else
            {
                var ex = Assert.Throws<InvalidOperationException>(() => _sut.BuildSequence());
                Console.WriteLine(ex.Message);
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
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    testCases = new JsonSerializer().Deserialize<BuildSequenceCase[]>(reader);
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

        public sealed class BuildSequenceCase
        {
            public string Name { get; set; }

            public ModuleVersion[] Version { get; set; }

            public SourceFile[] Files { get; set; }

            public string[] Sequence { get; set; }

            public string[] Exception { get; set; }
        }

        public sealed class ModuleVersion
        {
            public string Module { get; set; }

            public string Version { get; set; }
        }

        public sealed class SourceFile
        {
            public string Name { get; set; }

            public ModuleVersion[] Dependencies { get; set; }
        }
    }
}