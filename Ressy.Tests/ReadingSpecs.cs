using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using Ressy.Identification;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public record ReadingSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_get_a_list_of_resources_stored_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            var resources = PortableExecutable.GetResources(imageFilePath);

            // Assert
            resources.Should().BeEquivalentTo(
                // -- RT_ICON/1/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(1),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/2/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(2),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/3/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(3),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/4/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(4),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/5/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(5),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/6/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(6),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/7/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(7),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/8/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(8),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/9/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(9),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/10/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(10),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/11/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(11),
                    ResourceLanguage.Neutral
                ),

                // -- RT_ICON/12/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    ResourceName.FromCode(12),
                    ResourceLanguage.Neutral
                ),

                // -- RT_STRING/7/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.String),
                    ResourceName.FromCode(7),
                    ResourceLanguage.Neutral
                ),

                // -- RT_STRING/7/Ukrainian (UA)
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.String),
                    ResourceName.FromCode(7),
                    new ResourceLanguage(1058)
                ),

                // -- RT_GROUP_ICON/1/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.GroupIcon),
                    ResourceName.FromCode(1),
                    ResourceLanguage.Neutral
                ),

                // -- RT_VERSION/1/English (US)
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Version),
                    ResourceName.FromCode(1),
                    ResourceLanguage.EnglishUnitedStates
                )
            );
        }

        [Fact]
        public void User_can_get_a_list_of_resources_stored_in_an_empty_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            // Act
            var resources = PortableExecutable.GetResources(imageFilePath);

            // Assert
            resources.Should().BeEmpty();
        }

        [Fact]
        public void User_can_get_label_of_a_type_of_resource_stored_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            var identifier = PortableExecutable
                .GetResources(imageFilePath)
                .First(r => r.Type.Code == (int) StandardResourceTypeCode.GroupIcon);

            // Assert
            identifier.Type.Label.Should().Be("#14 (GROUP_ICON)");
        }

        [Fact]
        public void User_can_read_a_specific_resource_stored_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            var data = PortableExecutable.GetResourceData(
                imageFilePath,
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.String),
                    ResourceName.FromCode(7),
                    new ResourceLanguage(1058)
                )
            );

            var dataText = Encoding.Unicode.GetString(data);

            // Assert
            dataText.Should().Contain("Привіт, світ");
        }

        [Fact]
        public void User_can_try_to_read_a_non_existing_resource_in_a_portable_executable_and_receive_an_exception()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            // Act & assert
            Assert.ThrowsAny<Exception>(() =>
                PortableExecutable.GetResourceData(
                    imageFilePath,
                    new ResourceIdentifier(
                        ResourceType.FromCode(1),
                        ResourceName.FromCode(1),
                        ResourceLanguage.Neutral
                    )
                )
            );
        }
    }
}