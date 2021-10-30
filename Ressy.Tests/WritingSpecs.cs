using FluentAssertions;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public record WritingSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_add_a_resource_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(descriptor, new byte[] { 1, 2, 3, 4, 5 });
            });

            var data = PortableExecutable.GetResourceData(imageFilePath, descriptor);

            // Assert
            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_custom_type_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromString("FOO"),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(descriptor, new byte[] { 1, 2, 3, 4, 5 });
            });

            var data = PortableExecutable.GetResourceData(imageFilePath, descriptor);

            // Assert
            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_custom_name_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromCode(6),
                ResourceName.FromString("BAR")
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(descriptor, new byte[] { 1, 2, 3, 4, 5 });
            });

            var data = PortableExecutable.GetResourceData(imageFilePath, descriptor);

            // Assert
            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_custom_type_and_name_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromString("FOO"),
                ResourceName.FromString("BAR")
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(descriptor, new byte[] { 1, 2, 3, 4, 5 });
            });

            var data = PortableExecutable.GetResourceData(imageFilePath, descriptor);

            // Assert
            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_overwrite_a_specific_resource_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(descriptor, new byte[] { 1, 2, 3, 4, 5 });
            });

            var data = PortableExecutable.GetResourceData(imageFilePath, descriptor);

            // Assert
            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_remove_a_specific_resource_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            var descriptor = new ResourceDescriptor(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Remove(descriptor);
            });

            // Assert
            PortableExecutable.GetResources(imageFilePath).Should().NotContain(descriptor);
        }

        [Fact]
        public void User_can_clear_resources_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            PortableExecutable.ClearResources(imageFilePath);

            // Assert
            PortableExecutable.GetResources(imageFilePath).Should().BeEmpty();
        }
    }
}