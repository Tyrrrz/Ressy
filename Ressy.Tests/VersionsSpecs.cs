using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ressy.Abstractions.Versions;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public record VersionsSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_get_the_application_version()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var version = portableExecutable.GetVersionInfo();

            // Assert
            version.FileVersion.Should().Be(new Version(1, 2, 3, 0));
            version.ProductVersion.Should().Be(new Version(1, 2, 3, 0));
            version.FileFlags.Should().Be(FileFlags.None);
            version.FileOperatingSystem.Should().Be(FileOperatingSystem.NT | FileOperatingSystem.Windows32);
            version.FileType.Should().Be(FileType.App);
            version.FileSubtype.Should().Be(FileSubType.Unknown);
            version.FileTimestamp.Should().Be(new DateTimeOffset(1601, 01, 01, 00, 00, 00, TimeSpan.Zero));
            version.Attributes.Should().BeEquivalentTo(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CompanyName"] = "My Company",
                ["FileDescription"] = "My Application",
                ["FileVersion"] = "1.2.3",
                ["LegalCopyright"] = "Copyright (C) 2021 My Company. All rights reserved.",
                ["ProductName"] = "My Product",
                ["ProductVersion"] = "1.2.3"
            });
            version.Translations.Should().BeEquivalentTo(new[]
            {
                new TranslationInfo(1033, 1252)
            });
        }

        [Fact]
        public void User_can_add_an_application_version()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

            var version = new VersionInfo(
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

            // Act
            portableExecutable.SetVersionInfo(version);

            // Assert
            portableExecutable.GetVersionInfo().Should().BeEquivalentTo(version);
        }

        [Fact]
        public void User_can_remove_the_application_version()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

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