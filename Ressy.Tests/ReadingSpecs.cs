﻿using System.Text;
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
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

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

                // -- RT_GROUP_ICON/32512/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.GroupIcon),
                    ResourceName.FromCode(32512),
                    ResourceLanguage.Neutral
                ),

                // -- RT_VERSION/1/Neutral
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Version),
                    ResourceName.FromCode(1)
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
        public void User_can_get_a_specific_resource()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            var resource = portableExecutable.GetResource(new ResourceIdentifier(
                ResourceType.FromCode(StandardResourceTypeCode.Manifest),
                ResourceName.FromCode(1)
            ));

            // Assert
            resource.ReadAsString(Encoding.UTF8).Should().Contain("assemblyIdentity");
        }

        [Fact]
        public void User_can_try_to_get_a_non_existing_resource_and_receive_null_instead()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

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