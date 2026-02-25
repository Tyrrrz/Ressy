using System.IO;
using FluentAssertions;
using Ressy.MultilingualUserInterface;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class MuiSpecs
{
    [Fact]
    public void I_can_set_the_mui_info()
    {
        // Arrange
        var muiInfo = new MultilingualUserInterfaceInfo(
            MuiFileType.LanguageSpecific,
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
    public void I_can_set_the_mui_info_with_no_languages()
    {
        // Arrange
        var muiInfo = new MultilingualUserInterfaceInfo(
            MuiFileType.LanguageNeutral,
            null,
            null,
            null
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
    public void I_can_remove_the_mui_info()
    {
        // Arrange
        var muiInfo = new MultilingualUserInterfaceInfo(
            MuiFileType.LanguageSpecific,
            "en-US",
            "en-US",
            null
        );

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);
        portableExecutable.SetMuiInfo(muiInfo);

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
