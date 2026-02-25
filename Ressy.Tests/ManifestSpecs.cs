using System.IO;
using FluentAssertions;
using Ressy.HighLevel.Manifests;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class ManifestSpecs
{
    [Fact]
    public void I_can_get_the_manifest()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var manifest = portableExecutable.GetManifest();

        // Assert
        manifest.Should().Contain("assemblyIdentity");
    }

    [Fact]
    public void I_can_set_the_manifest()
    {
        // Arrange
        const string manifest =
            // lang=xml
            """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
                    <assemblyIdentity
                        name="MyAssembly"
                        processorArchitecture="x86"
                        version="1.0.0.0"
                        type="win32" />
                </assembly>
                """;

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveManifest();

        // Act
        portableExecutable.SetManifest(manifest);

        // Assert
        portableExecutable.GetManifest().Should().Be(manifest);
    }

    [Fact]
    public void I_can_remove_the_manifest()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveManifest();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Code == ResourceType.Manifest.Code);

        portableExecutable.TryGetManifest().Should().BeNull();
    }
}
