using System.Globalization;
using FluentAssertions;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests;

public class WritingSpecs : IClassFixture<DummyFixture>
{
    private readonly DummyFixture _dummy;

    public WritingSpecs(DummyFixture dummy) => _dummy = dummy;

    [Fact]
    public void User_can_add_a_resource()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromCode(6),
            ResourceName.FromCode(7)
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_add_a_resource_with_a_non_standard_ordinal_type()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromCode(420),
            ResourceName.FromCode(7)
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_add_a_resource_with_a_non_ordinal_type()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromString("FOO"),
            ResourceName.FromCode(7)
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_add_a_resource_with_a_non_ordinal_name()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromCode(6),
            ResourceName.FromString("BAR")
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_add_a_resource_with_a_non_ordinal_type_and_non_ordinal_name()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromString("FOO"),
            ResourceName.FromString("BAR")
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_add_a_resource_with_a_custom_language()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromCode(6),
            ResourceName.FromCode(7),
            Language.FromCultureInfo(CultureInfo.GetCultureInfo("uk-UA"))
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
        portableExecutable.ClearResources();

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_overwrite_a_specific_resource()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.Manifest,
            ResourceName.FromCode(1)
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

        // Act
        portableExecutable.SetResource(identifier, new byte[] { 1, 2, 3, 4, 5 });

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().ContainSingle(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );

        portableExecutable.GetResource(identifier).Data.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void User_can_remove_a_specific_resource()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.Manifest,
            ResourceName.FromCode(1)
        );

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

        // Act
        portableExecutable.RemoveResource(identifier);

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().NotContain(r =>
            r.Type.Code == identifier.Type.Code &&
            r.Type.Label == identifier.Type.Label &&
            r.Name.Code == identifier.Name.Code &&
            r.Name.Label == identifier.Name.Label &&
            r.Language.Id == identifier.Language.Id
        );
    }

    [Fact]
    public void User_can_clear_resources()
    {
        // Arrange
        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

        // Act
        portableExecutable.ClearResources();

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().BeEmpty();
    }
}