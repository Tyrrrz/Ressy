using System.Globalization;
using System.IO;
using FluentAssertions;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class WritingSpecs
{
    [Fact]
    public void User_can_add_a_resource()
    {
        // Arrange
        var identifier = new ResourceIdentifier(
            ResourceType.FromCode(6),
            ResourceName.FromCode(7)
        );

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

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

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

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
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.ClearResources();

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().BeEmpty();
    }
}