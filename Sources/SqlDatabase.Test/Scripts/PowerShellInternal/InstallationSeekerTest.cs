using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;
using InstallationInfo = SqlDatabase.Scripts.PowerShellInternal.InstallationSeeker.InstallationInfo;

namespace SqlDatabase.Scripts.PowerShellInternal
{
    [TestFixture]
    public class InstallationSeekerTest
    {
        [Test]
        public void TryFindByParentProcess()
        {
            var actual = InstallationSeeker.TryFindByParentProcess(out var path);
            if (actual)
            {
                Console.WriteLine(path);
            }
        }

        [Test]
        public void TryFindOnDisk()
        {
#if NET472
            Assert.Ignore();
#endif

            InstallationSeeker.TryFindOnDisk(out var path).ShouldBeTrue();

            Console.WriteLine(path);
        }

        [Test]
        public void TryGetInfo()
        {
            using (var dir = new TempDirectory())
            {
                var root = Path.Combine(dir.Location, InstallationSeeker.RootAssemblyFileName);

                InstallationSeeker.TryGetInfo(dir.Location, out _).ShouldBeFalse();

                File.WriteAllText(Path.Combine(dir.Location, "pwsh.dll"), "dummy");
                File.WriteAllText(root, "dummy");

                InstallationSeeker.TryGetInfo(dir.Location, out _).ShouldBeFalse();

                File.Delete(root);
                File.Copy(GetType().Assembly.Location, root);

                InstallationSeeker.TryGetInfo(dir.Location, out var actual).ShouldBeTrue();

                actual.Location.ShouldBe(dir.Location);
                actual.Version.ShouldBe(GetType().Assembly.GetName().Version);
                actual.ProductVersion.ShouldBe(actual.Version.ToString());
            }
        }

        [Test]
        [TestCaseSource(nameof(GetSortInstallationInfoCases))]
        public void SortInstallationInfo(object item1, object item2)
        {
            var info1 = (InstallationInfo)item1;
            var info2 = (InstallationInfo)item2;

            var comparer = new Mock<IEqualityComparer<InstallationInfo>>(MockBehavior.Strict);
            comparer
                .Setup(c => c.Equals(It.IsAny<InstallationInfo>(), It.IsAny<InstallationInfo>()))
                .Returns<InstallationInfo, InstallationInfo>((x, y) =>
                {
                    return x.Location.Equals(y.Location, StringComparison.OrdinalIgnoreCase)
                           && x.Version == y.Version
                           && x.ProductVersion.Equals(y.ProductVersion, StringComparison.OrdinalIgnoreCase);
                });

            var list = new List<InstallationInfo> { info1, info2 };
            list.Sort();
            list[1].ShouldBe(info2, comparer.Object);

            list = new List<InstallationInfo> { info2, info1 };
            list.Sort();
            list[1].ShouldBe(info2, comparer.Object);
        }

        private static IEnumerable<TestCaseData> GetSortInstallationInfoCases()
        {
            yield return new TestCaseData(
                new InstallationInfo("path", new Version(1, 0), "1.0"),
                new InstallationInfo("path", new Version(2, 0), "2.0"))
            {
                TestName = "1.0 vs 2.0"
            };

            yield return new TestCaseData(
                new InstallationInfo("path", new Version(1, 0), "1.0-preview"),
                new InstallationInfo("path", new Version(1, 0), "1.0"))
            {
                TestName = "1.0-preview vs 1.0"
            };

            yield return new TestCaseData(
                new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"),
                new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"))
            {
                TestName = "1.0-preview.1 vs 1.0-preview.2"
            };

            yield return new TestCaseData(
                new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"),
                new InstallationInfo("path", new Version(1, 0), "1.0-preview.2"))
            {
                TestName = "1.0-preview.1 vs 1.0-preview.2"
            };

            yield return new TestCaseData(
                new InstallationInfo("path 1", new Version(1, 0), "1.0-preview.1"),
                new InstallationInfo("path 2", new Version(1, 0), "1.0-preview.1"))
            {
                TestName = "1.0-preview.1 vs 1.0-preview.1"
            };
        }
    }
}
