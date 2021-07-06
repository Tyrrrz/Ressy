using System;
using FluentAssertions;
using Ressy.Tests.Dummy;
using Xunit;

namespace Ressy.Tests
{
    public class ReadingSpecs
    {
        [Fact]
        public void User_can_get_a_list_of_resources_from_an_empty_file()
        {
            // Arrange
            using var dummy = DummyPortableExecutable.Create();
            using var executable = PortableExecutable.FromFile(dummy.FilePath);

            // Act
            var resources = executable.GetResources();

            // Assert
            resources.Should().BeEmpty();
        }

        [Fact]
        public void User_can_try_to_safely_get_a_non_existing_resource_which_returns_null()
        {
            // Arrange
            using var dummy = DummyPortableExecutable.Create();
            using var executable = PortableExecutable.FromFile(dummy.FilePath);

            // Act
            var resource = executable.TryGetResource(
                ResourceType.FromString("#1"),
                ResourceName.FromString("#1"),
                ResourceLanguage.Neutral
            );

            // Assert
            resource.Should().BeNull();
        }

        [Fact]
        public void User_can_try_to_get_a_non_existing_resource_which_throws()
        {
            // Arrange
            using var dummy = DummyPortableExecutable.Create();
            using var executable = PortableExecutable.FromFile(dummy.FilePath);

            // Act & assert
            Assert.ThrowsAny<Exception>(() =>
                executable.GetResource(
                    ResourceType.FromString("#1"),
                    ResourceName.FromString("#1"),
                    ResourceLanguage.Neutral
                )
            );
        }
    }
}