using System;
using System.Diagnostics;
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
        // The MUI resource is not embedded in the dummy PE via .rc because
        // its presence causes Windows to redirect FileVersionInfo lookups to
        // a satellite .mui file, which doesn't exist at the test path.
        // Instead, we inject it through Ressy's API and then read it back.

        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var original = new MuiInfo(
            MuiFileType.LanguageNeutral,
            checksum: new byte[16],
            serviceChecksum: new byte[16],
            mainResourceTypes: [ResourceType.Version],
            fallbackResourceTypes: [],
            null,
            null,
            "en"
        );

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);
        portableExecutable.SetMuiInfo(original);

        // Act
        var muiInfo = portableExecutable.GetMuiInfo();

        // Assert
        muiInfo.FileType.Should().Be(MuiFileType.LanguageNeutral);
        muiInfo.Language.Should().BeNull();
        muiInfo.FallbackLanguage.Should().BeNull();
        muiInfo.UltimateFallbackLanguage.Should().Be("en");
        muiInfo.MainResourceTypes.Should().Equal(ResourceType.Version);
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

        // Act
        portableExecutable.SetMuiInfo(muiInfo);

        // Assert
        portableExecutable.GetMuiInfo().Should().BeEquivalentTo(muiInfo);
    }

    [Fact]
    public void I_can_overwrite_the_MUI_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);

        // Set an initial MUI resource
        portableExecutable.SetMuiInfo(
            new MuiInfo(
                MuiFileType.LanguageNeutral,
                new byte[16],
                new byte[16],
                [ResourceType.Version],
                [],
                null,
                null,
                "en"
            )
        );

        // Act — overwrite with different data
        var updated = new MuiInfo(
            MuiFileType.LanguageSpecific,
            new byte[16],
            new byte[16],
            [ResourceType.String],
            [],
            "fr-FR",
            "fr-FR",
            "fr"
        );
        portableExecutable.SetMuiInfo(updated);

        // Assert
        portableExecutable.GetMuiInfo().Should().BeEquivalentTo(updated);
    }

    [Fact]
    public void I_can_remove_the_MUI_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);

        // Inject a MUI resource so there's something to remove
        portableExecutable.SetMuiInfo(
            new MuiInfo(
                MuiFileType.LanguageNeutral,
                new byte[16],
                new byte[16],
                [ResourceType.Version],
                [],
                null,
                null,
                "en"
            )
        );

        // Act
        portableExecutable.RemoveMuiInfo();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Equals(ResourceType.Mui));

        portableExecutable.TryGetMuiInfo().Should().BeNull();
    }

    [Fact]
    public void I_can_get_the_MUI_info_and_FileVersionInfo_still_works()
    {
        // Verify that injecting a MUI resource via Ressy's API does not
        // break Windows FileVersionInfo when MainResourceTypes includes
        // RT_VERSION (indicating version info stays in the neutral file).
        if (!OperatingSystem.IsWindows())
            return;

        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using (var portableExecutable = PortableExecutable.OpenWrite(file.Path))
        {
            portableExecutable.SetMuiInfo(
                new MuiInfo(
                    MuiFileType.LanguageNeutral,
                    checksum: new byte[16],
                    serviceChecksum: new byte[16],
                    mainResourceTypes: [ResourceType.Version],
                    fallbackResourceTypes: [],
                    null,
                    null,
                    "en"
                )
            );
        }

        // Act
        var versionInfo = FileVersionInfo.GetVersionInfo(file.Path);

        // Assert
        versionInfo.ProductName.Should().Be("TestProduct");
        versionInfo.FileDescription.Should().Be("TestDescription");
        versionInfo.CompanyName.Should().Be("TestCompany");
        versionInfo.FileVersion.Should().Be("1.2.3.4");
    }

    [Fact]
    public void I_can_get_more_resources_from_notepad_with_satellites()
    {
        // On Windows, notepad.exe has satellite .mui files in language subdirectories.
        // Opening with satellites should yield more resource identifiers.
        if (!OperatingSystem.IsWindows())
            return;

        var notepadPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.System),
            "notepad.exe"
        );
        if (!File.Exists(notepadPath))
            return;

        // Act
        int countWithout;
        using (var peWithout = PortableExecutable.OpenRead(notepadPath))
            countWithout = peWithout.GetResourceIdentifiers().Count;

        int countWith;
        using (var peWith = PortableExecutable.OpenRead(notepadPath, openSatellites: true))
            countWith = peWith.GetResourceIdentifiers().Count;

        // Assert
        countWith.Should().BeGreaterThan(countWithout);
    }
}
