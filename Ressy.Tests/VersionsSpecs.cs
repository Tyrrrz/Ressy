using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ressy.Abstractions.Versions;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public class VersionsSpecs : IClassFixture<DummyFixture>
    {
        private readonly DummyFixture _dummy;

        public VersionsSpecs(DummyFixture dummy) => _dummy = dummy;

        [Fact]
        public void User_can_get_the_application_version()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            var versionInfo = portableExecutable.GetVersionInfo();

            // Assert
            versionInfo.FileVersion.Should().Be(new Version(1, 2, 3, 4));
            versionInfo.ProductVersion.Should().Be(new Version(5, 6, 7, 8));
            versionInfo.FileFlags.Should().Be(FileFlags.None);
            versionInfo.FileOperatingSystem.Should().Be(FileOperatingSystem.Windows32);
            versionInfo.FileType.Should().Be(FileType.App);
            versionInfo.FileSubType.Should().Be(FileSubType.Unknown);
            versionInfo.FileTimestamp.Should().Be(new DateTimeOffset(1601, 01, 01, 00, 00, 00, TimeSpan.Zero));
            versionInfo.Attributes.Should().Contain(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assembly Version"] = "6.9.6.9",
                ["FileVersion"] = "1.2.3.4",
                ["ProductVersion"] = "5.6.7.8",
                ["ProductName"] = "TestProduct",
                ["FileDescription"] = "TestDescription",
                ["CompanyName"] = "TestCompany",
                ["Comments"] = "TestComments",
                ["LegalCopyright"] = "TestCopyright"
            });
            versionInfo.Translations.Should().BeEquivalentTo(new[]
            {
                new TranslationInfo(0, 1200)
            });
        }

        [Fact]
        public void User_can_add_an_application_version()
        {
            // Arrange
            var versionInfo = new VersionInfo(
                new Version(6, 7, 8, 9),
                new Version(2, 3, 1, 9),
                FileFlags.None,
                FileOperatingSystem.NT | FileOperatingSystem.Windows32,
                FileType.App,
                FileSubType.Unknown,
                new DateTimeOffset(2021, 11, 17, 00, 00, 00, TimeSpan.Zero),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Foo"] = "Bar",
                    ["Baz"] = "Boom"
                },
                new[] { new TranslationInfo(0, 1252) }
            );

            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
            portableExecutable.ClearResources();

            // Act
            portableExecutable.SetVersionInfo(versionInfo);

            // Assert
            portableExecutable.GetVersionInfo().Should().BeEquivalentTo(versionInfo);
        }

        [Fact]
        public void User_can_modify_the_application_version()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            portableExecutable.SetVersionInfo(v => v
                .SetFileVersion(new Version(4, 3, 2, 1))
                .SetFileOperatingSystem(FileOperatingSystem.Windows32 | FileOperatingSystem.NT)
                .SetAttribute(StandardVersionAttributeName.ProductName, "ProductTest")
                .SetAttribute(StandardVersionAttributeName.CompanyName, "CompanyTest")
            );

            // Assert
            var versionInfo = portableExecutable.GetVersionInfo();

            versionInfo.FileVersion.Should().Be(new Version(4, 3, 2, 1));
            versionInfo.ProductVersion.Should().Be(new Version(5, 6, 7, 8));
            versionInfo.FileFlags.Should().Be(FileFlags.None);
            versionInfo.FileOperatingSystem.Should().Be(FileOperatingSystem.Windows32 | FileOperatingSystem.NT);
            versionInfo.FileType.Should().Be(FileType.App);
            versionInfo.FileSubType.Should().Be(FileSubType.Unknown);
            versionInfo.FileTimestamp.Should().Be(new DateTimeOffset(1601, 01, 01, 00, 00, 00, TimeSpan.Zero));
            versionInfo.Attributes.Should().Contain(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assembly Version"] = "6.9.6.9",
                ["FileVersion"] = "4.3.2.1",
                ["ProductVersion"] = "5.6.7.8",
                ["ProductName"] = "ProductTest",
                ["FileDescription"] = "TestDescription",
                ["CompanyName"] = "CompanyTest",
                ["Comments"] = "TestComments",
                ["LegalCopyright"] = "TestCopyright"
            });
            versionInfo.Translations.Should().BeEquivalentTo(new[]
            {
                new TranslationInfo(0, 1200)
            });
        }

        [Fact]
        public void User_can_remove_the_application_version()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            portableExecutable.RemoveVersionInfo();

            // Assert
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == (int)StandardResourceTypeCode.Version)
                .Should()
                .BeEmpty();
        }
    }
}