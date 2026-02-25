using System.IO;
using System.Text;
using FluentAssertions;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class ReadingSpecs
{
    [Fact]
    public void I_can_get_a_list_of_resource_identifiers()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var identifiers = portableExecutable.GetResourceIdentifiers();

        // Assert
        identifiers
            .Should()
            .BeEquivalentTo([
                // -- RT_ICON/1/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(1),
                    Language.Neutral
                ),
                // -- RT_ICON/2/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(2),
                    Language.Neutral
                ),
                // -- RT_ICON/3/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(3),
                    Language.Neutral
                ),
                // -- RT_ICON/4/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(4),
                    Language.Neutral
                ),
                // -- RT_ICON/5/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(5),
                    Language.Neutral
                ),
                // -- RT_ICON/6/Neutral
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(6),
                    Language.Neutral
                ),
                // -- RT_GROUP_ICON/1/Neutral
                new ResourceIdentifier(
                    ResourceType.IconGroup,
                    ResourceName.FromCode(1),
                    Language.Neutral
                ),
                // -- RT_MANIFEST/1/Neutral
                new ResourceIdentifier(
                    ResourceType.Manifest,
                    ResourceName.FromCode(1),
                    Language.Neutral
                ),
                // -- RT_VERSION/1/Neutral
                new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1)),
                // -- RT_STRING/1/English
                new ResourceIdentifier(
                    ResourceType.String,
                    ResourceName.FromCode(1),
                    Language.NeutralDefault
                ),
                // -- RT_STRING/2/English
                new ResourceIdentifier(
                    ResourceType.String,
                    ResourceName.FromCode(2),
                    Language.NeutralDefault
                ),
                // -- RT_STRING/1/French
                new ResourceIdentifier(
                    ResourceType.String,
                    ResourceName.FromCode(1),
                    new Language(1036)
                ),
                // -- RT_STRING/2/French
                new ResourceIdentifier(
                    ResourceType.String,
                    ResourceName.FromCode(2),
                    new Language(1036)
                ),
            ]);
    }

    [Fact]
    public void I_can_get_a_specific_resource()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var resource = portableExecutable.GetResource(
            new ResourceIdentifier(ResourceType.Manifest, ResourceName.FromCode(1))
        );

        // Assert
        resource.ReadAsString(Encoding.UTF8).Should().Contain("assemblyIdentity");
    }

    [Fact]
    public void I_can_try_to_get_a_non_existing_resource_and_receive_null_instead()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var resource = portableExecutable.TryGetResource(
            new ResourceIdentifier(
                ResourceType.FromCode(1),
                ResourceName.FromCode(1),
                Language.Neutral
            )
        );

        // Assert
        resource.Should().BeNull();
    }
}
