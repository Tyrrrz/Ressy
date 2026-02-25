using System.IO;
using FluentAssertions;
using Ressy.MultilingualUserInterface;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class MuiSpecs
{
    [Fact]
    public void I_can_get_the_MUI_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenRead(file.Path);

        // Act
        var muiInfo = portableExecutable.GetMuiInfo();

        // Assert
        muiInfo.FileType.Should().Be(MuiFileType.LanguageNeutral);
        muiInfo.Language.Should().BeNull();
        muiInfo.FallbackLanguage.Should().BeNull();
        muiInfo.UltimateFallbackLanguage.Should().Be("en");
        muiInfo.MainResourceTypes.Should().BeEmpty();
        muiInfo.FallbackResourceTypes.Should().BeEmpty();
    }

    [Fact]
    public void I_can_set_the_MUI_info()
    {
        // Arrange
        var muiInfo = new MuiInfo(
            MuiFileType.LanguageSpecific,
            checksum: new byte[16],
            serviceChecksum: new byte[16],
            mainResourceTypes: [ResourceType.String, ResourceType.Version],
            fallbackResourceTypes: [],
            "en-US",
            "en-US",
            "en"
        );

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);
        portableExecutable.RemoveMuiInfo();

        // Act
        portableExecutable.SetMuiInfo(muiInfo);

        // Assert
        portableExecutable.GetMuiInfo().Should().BeEquivalentTo(muiInfo);
    }

    [Fact]
    public void I_can_remove_the_MUI_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);

        // Act
        portableExecutable.RemoveMuiInfo();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Equals(ResourceType.Mui));

        portableExecutable.TryGetMuiInfo().Should().BeNull();
    }
}
