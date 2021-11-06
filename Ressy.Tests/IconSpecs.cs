using System.IO;
using System.Linq;
using FluentAssertions;
using Ressy.Identification;
using Ressy.Tests.Fixtures;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests
{
    public record IconSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_add_an_icon_to_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();
            var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

            // Act
            PortableExecutable.SetIcon(imageFilePath, iconFilePath);

            // Assert
            PortableExecutable.GetResources(imageFilePath).Should().BeEquivalentTo(
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
            );
        }

        [Fact]
        public void User_can_overwrite_an_icon_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();
            var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

            // Act
            PortableExecutable.SetIcon(imageFilePath, iconFilePath, ResourceLanguage.Neutral);

            // Assert
            PortableExecutable
                .GetResources(imageFilePath)
                .Where(
                    r => r.Type.Code is (int)StandardResourceTypeCode.GroupIcon or (int)StandardResourceTypeCode.Icon
                )
                .Should()
                .BeEquivalentTo(
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
                );
        }
    }
}