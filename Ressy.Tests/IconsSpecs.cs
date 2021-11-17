using System.IO;
using System.Linq;
using FluentAssertions;
using Ressy.Abstractions.Icons;
using Ressy.Tests.Fixtures;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests
{
    public record IconsSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_add_an_application_icon()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

            var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

            // Act
            portableExecutable.SetIcon(iconFilePath);

            // Assert
            portableExecutable.GetResourceIdentifiers().Should().BeEquivalentTo(new[]
            {
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.GroupIcon),
                    ResourceName.FromCode(1),
                    ResourceLanguage.Neutral
                ),

                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(1),
                    ResourceLanguage.Neutral
                ),

                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(2),
                    ResourceLanguage.Neutral
                ),

                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(3),
                    ResourceLanguage.Neutral
                ),

                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(4),
                    ResourceLanguage.Neutral
                ),

                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(5),
                    ResourceLanguage.Neutral
                )
            });
        }

        [Fact]
        public void User_can_overwrite_the_application_icon()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

            var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

            // Act
            portableExecutable.SetIcon(iconFilePath);

            // Assert
            portableExecutable
                .GetResourceIdentifiers()
                .Where(
                    r => r.Type.Code is
                        (int)StandardResourceTypeCode.GroupIcon or
                        (int)StandardResourceTypeCode.Icon
                )
                .Should()
                .BeEquivalentTo(new[]
                {
                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.GroupIcon),
                        ResourceName.FromCode(1),
                        ResourceLanguage.Neutral
                    ),

                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Icon),
                        ResourceName.FromCode(1),
                        ResourceLanguage.Neutral
                    ),

                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Icon),
                        ResourceName.FromCode(2),
                        ResourceLanguage.Neutral
                    ),

                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Icon),
                        ResourceName.FromCode(3),
                        ResourceLanguage.Neutral
                    ),

                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Icon),
                        ResourceName.FromCode(4),
                        ResourceLanguage.Neutral
                    ),

                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Icon),
                        ResourceName.FromCode(5),
                        ResourceLanguage.Neutral
                    )
                });
        }

        [Fact]
        public void User_can_remove_the_application_icon()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();
            using var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            portableExecutable.RemoveIcon();

            // Assert
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code is
                    (int)StandardResourceTypeCode.GroupIcon or
                    (int)StandardResourceTypeCode.Icon
                )
                .Should()
                .BeEmpty();
        }
    }
}