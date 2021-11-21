using System.Globalization;
using FluentAssertions;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public class ReadingSpecs : IClassFixture<DummyFixture>
    {
        private readonly DummyFixture _dummy;

        public ReadingSpecs(DummyFixture dummy) => _dummy = dummy;

        [Fact]
        public void User_can_get_a_list_of_resource_identifiers()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var identifiers = portableExecutable.GetResourceIdentifiers();

            // Assert
            identifiers.Should().BeEquivalentTo(new[]
            {
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
                    ResourceLanguage.FromCultureInfo(new CultureInfo("en-US"))
                ),

                // -- RT_MANIFEST/1/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Manifest),
                    ResourceName.FromCode(1),
                    ResourceLanguage.Neutral
                )
            });
        }

        [Fact]
        public void User_can_get_a_list_of_resource_identifiers_in_an_empty_image()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithoutResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var identifiers = portableExecutable.GetResourceIdentifiers();

            // Assert
            identifiers.Should().BeEmpty();
        }

        [Fact]
        public void User_can_get_a_specific_resource()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var resource = portableExecutable.GetResource(new ResourceIdentifier(
                ResourceType.FromCode(StandardResourceTypeCode.String),
                ResourceName.FromCode(7),
                ResourceLanguage.FromCultureInfo(CultureInfo.GetCultureInfo("uk-UA"))
            ));

            // Assert
            resource.ReadAsString().Should().Contain("Привіт, світ");
        }

        [Fact]
        public void User_can_try_to_get_a_non_existing_resource_and_receive_null_instead()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithoutResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var resource = portableExecutable.TryGetResource(new ResourceIdentifier(
                ResourceType.FromCode(1),
                ResourceName.FromCode(1),
                ResourceLanguage.Neutral
            ));

            // Assert
            resource.Should().BeNull();
        }
    }
}