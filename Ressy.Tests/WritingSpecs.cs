using FluentAssertions;
using Ressy.Identification;
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

            var identifier = new ResourceIdentifier(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_non_standard_ordinal_type_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromCode(420),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_non_ordinal_type_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromString("FOO"),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_non_ordinal_name_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromCode(6),
                ResourceName.FromString("BAR")
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_add_a_resource_with_a_non_ordinal_type_and_non_ordinal_name_to_the_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromString("FOO"),
                ResourceName.FromString("BAR")
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_overwrite_a_specific_resource_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Set(identifier, new byte[] { 1, 2, 3, 4, 5 });
            });

            var resources = PortableExecutable.GetResources(imageFilePath);
            var data = PortableExecutable.GetResourceData(imageFilePath, identifier);

            // Assert
            resources.Should().ContainSingle(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );

            data.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public void User_can_remove_a_specific_resource_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            var identifier = new ResourceIdentifier(
                ResourceType.FromCode(6),
                ResourceName.FromCode(7)
            );

            // Act
            PortableExecutable.UpdateResources(imageFilePath, ctx =>
            {
                ctx.Remove(identifier);
            });

            var resources = PortableExecutable.GetResources(imageFilePath);

            // Assert
            resources.Should().NotContain(r =>
                r.Type.Code == identifier.Type.Code &&
                r.Type.Label == identifier.Type.Label &&
                r.Name.Code == identifier.Name.Code &&
                r.Name.Label == identifier.Name.Label &&
                r.Language.Id == identifier.Language.Id
            );
        }

        [Fact]
        public void User_can_clear_resources_in_a_portable_executable()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            PortableExecutable.ClearResources(imageFilePath);

            var resources = PortableExecutable.GetResources(imageFilePath);

            // Assert
            resources.Should().BeEmpty();
        }
    }
}