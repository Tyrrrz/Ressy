using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            versionInfo.Attributes.Should().Contain(new Dictionary<VersionAttributeName, string>
            {
                ["Assembly Version"] = "6.9.6.9",
                [VersionAttributeName.FileVersion] = "1.2.3.4",
                [VersionAttributeName.ProductVersion] = "5.6.7.8",
                [VersionAttributeName.ProductName] = "TestProduct",
                [VersionAttributeName.FileDescription] = "TestDescription",
                [VersionAttributeName.CompanyName] = "TestCompany",
                [VersionAttributeName.Comments] = "TestComments",
                [VersionAttributeName.LegalCopyright] = "TestCopyright"
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
            var versionInfo = new VersionInfoBuilder()
                .SetFileVersion(new Version(6, 7, 8, 9))
                .SetProductVersion(new Version(2, 3, 1, 9))
                .SetFileOperatingSystem(FileOperatingSystem.Windows32 | FileOperatingSystem.NT)
                .SetAttribute(VersionAttributeName.ProductName, "Foo")
                .SetAttribute(VersionAttributeName.FileDescription, "Bar")
                .SetAttribute(VersionAttributeName.CompanyName, "Baz")
                .SetAttribute("Custom", "Value")
                .Build();

            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
            portableExecutable.RemoveVersionInfo();

            // Act
            portableExecutable.SetVersionInfo(versionInfo);

            // Assert
            portableExecutable.GetVersionInfo().Should().BeEquivalentTo(versionInfo);

            var actualVersionInfo = FileVersionInfo.GetVersionInfo(portableExecutable.FilePath);
            actualVersionInfo.FileVersion.Should().Be(versionInfo.FileVersion.ToString(4));
            actualVersionInfo.ProductVersion.Should().Be(versionInfo.ProductVersion.ToString(4));
            actualVersionInfo.ProductName.Should().Be(versionInfo.Attributes[VersionAttributeName.ProductName]);
            actualVersionInfo.FileDescription.Should().Be(versionInfo.Attributes[VersionAttributeName.FileDescription]);
            actualVersionInfo.CompanyName.Should().Be(versionInfo.Attributes[VersionAttributeName.CompanyName]);
            actualVersionInfo.Comments.Should().BeNullOrEmpty();
            actualVersionInfo.LegalCopyright.Should().BeNullOrEmpty();
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
                .SetAttribute(VersionAttributeName.ProductName, "ProductTest")
                .SetAttribute(VersionAttributeName.CompanyName, "CompanyTest")
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
            versionInfo.Attributes.Should().Contain(new Dictionary<VersionAttributeName, string>
            {
                ["Assembly Version"] = "6.9.6.9",
                [VersionAttributeName.FileVersion] = "4.3.2.1",
                [VersionAttributeName.ProductVersion] = "5.6.7.8",
                [VersionAttributeName.ProductName] = "ProductTest",
                [VersionAttributeName.FileDescription] = "TestDescription",
                [VersionAttributeName.CompanyName] = "CompanyTest",
                [VersionAttributeName.Comments] = "TestComments",
                [VersionAttributeName.LegalCopyright] = "TestCopyright"
            });
            versionInfo.Translations.Should().BeEquivalentTo(new[]
            {
                new TranslationInfo(0, 1200)
            });

            var actualVersionInfo = FileVersionInfo.GetVersionInfo(portableExecutable.FilePath);
            actualVersionInfo.FileVersion.Should().Be(versionInfo.FileVersion.ToString(4));
            actualVersionInfo.ProductVersion.Should().Be(versionInfo.ProductVersion.ToString(4));
            actualVersionInfo.ProductName.Should().Be(versionInfo.Attributes[VersionAttributeName.ProductName]);
            actualVersionInfo.FileDescription.Should().Be(versionInfo.Attributes[VersionAttributeName.FileDescription]);
            actualVersionInfo.CompanyName.Should().Be(versionInfo.Attributes[VersionAttributeName.CompanyName]);
            actualVersionInfo.Comments.Should().Be(versionInfo.Attributes[VersionAttributeName.Comments]);
            actualVersionInfo.LegalCopyright.Should().Be(versionInfo.Attributes[VersionAttributeName.LegalCopyright]);
        }

        [Fact]
        public void User_can_remove_the_application_version()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            portableExecutable.RemoveVersionInfo();

            // Assert
            portableExecutable.GetResourceIdentifiers().Should().NotContain(
                r => r.Type.Code == ResourceType.Version.Code
            );

            var actualVersionInfo = FileVersionInfo.GetVersionInfo(portableExecutable.FilePath);
            actualVersionInfo.FileVersion.Should().BeNullOrEmpty();
            actualVersionInfo.ProductVersion.Should().BeNullOrEmpty();
            actualVersionInfo.ProductName.Should().BeNullOrEmpty();
            actualVersionInfo.FileDescription.Should().BeNullOrEmpty();
            actualVersionInfo.CompanyName.Should().BeNullOrEmpty();
            actualVersionInfo.Comments.Should().BeNullOrEmpty();
            actualVersionInfo.LegalCopyright.Should().BeNullOrEmpty();
        }
    }
}