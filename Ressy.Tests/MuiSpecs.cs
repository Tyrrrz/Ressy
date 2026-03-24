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
        // The rc compiler doesn't have a macro for MUI's binary structure, so it's
        // really difficult to inject a valid and consistent MUI resource outside of Ressy.
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
    public void I_can_get_resources_with_no_satellites()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        // Act
        using var pe = PortableExecutable.OpenRead(file.Path, openSatellites: true);
        var identifiers = pe.GetResourceIdentifiers();

        // Assert — should return the same as without satellites
        using var peBaseline = PortableExecutable.OpenRead(file.Path);
        identifiers.Should().BeEquivalentTo(peBaseline.GetResourceIdentifiers());
    }

    [Fact]
    public void I_can_get_resources_with_satellites()
    {
        // Arrange — create a directory with a main PE and a satellite
        using var dir = TempDir.Create();

        var mainPath = Path.Combine(dir.Path, "test.exe");
        File.Copy(Dummy.Program.Path, mainPath);

        var satelliteDir = Path.Combine(dir.Path, "fr-FR");
        Directory.CreateDirectory(satelliteDir);

        var satellitePath = Path.Combine(satelliteDir, "test.exe.mui");
        File.Copy(Dummy.Program.Path, satellitePath);

        // Add a unique resource to the satellite so we can detect it
        using (var satellitePe = PortableExecutable.OpenWrite(satellitePath))
        {
            satellitePe.SetResource(
                new Resource(
                    new ResourceIdentifier(
                        ResourceType.FromCode(255),
                        ResourceName.FromCode(1),
                        Language.Neutral
                    ),
                    new byte[] { 0xCA, 0xFE }
                )
            );
        }

        // Act
        using var pe = PortableExecutable.OpenRead(mainPath, openSatellites: true);
        var identifiers = pe.GetResourceIdentifiers();

        // Assert — should include the unique resource from the satellite
        identifiers
            .Should()
            .Contain(
                new ResourceIdentifier(
                    ResourceType.FromCode(255),
                    ResourceName.FromCode(1),
                    Language.Neutral
                )
            );
    }

    [Fact]
    public void I_can_get_satellite_resource_data()
    {
        // Arrange
        using var dir = TempDir.Create();

        var mainPath = Path.Combine(dir.Path, "test.exe");
        File.Copy(Dummy.Program.Path, mainPath);

        var satelliteDir = Path.Combine(dir.Path, "en-US");
        Directory.CreateDirectory(satelliteDir);

        var satellitePath = Path.Combine(satelliteDir, "test.exe.mui");
        File.Copy(Dummy.Program.Path, satellitePath);

        var expectedData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var resourceId = new ResourceIdentifier(
            ResourceType.FromCode(200),
            ResourceName.FromCode(1),
            Language.Neutral
        );

        using (var satellitePe = PortableExecutable.OpenWrite(satellitePath))
        {
            satellitePe.SetResource(new Resource(resourceId, expectedData));
        }

        // Act
        using var pe = PortableExecutable.OpenRead(mainPath, openSatellites: true);
        var resource = pe.TryGetResource(resourceId);

        // Assert
        resource.Should().NotBeNull();
        resource!.Data.Should().Equal(expectedData);
    }

    [Fact]
    public void Satellite_resource_overrides_main_resource()
    {
        // Arrange
        using var dir = TempDir.Create();

        var mainPath = Path.Combine(dir.Path, "test.exe");
        File.Copy(Dummy.Program.Path, mainPath);

        var resourceId = new ResourceIdentifier(
            ResourceType.FromCode(201),
            ResourceName.FromCode(1),
            Language.Neutral
        );

        // Add resource to the main file
        using (var mainPe = PortableExecutable.OpenWrite(mainPath))
        {
            mainPe.SetResource(new Resource(resourceId, new byte[] { 0x01 }));
        }

        // Add the same resource to a satellite with different data
        var satelliteDir = Path.Combine(dir.Path, "en-US");
        Directory.CreateDirectory(satelliteDir);

        var satellitePath = Path.Combine(satelliteDir, "test.exe.mui");
        File.Copy(mainPath, satellitePath);

        using (var satellitePe = PortableExecutable.OpenWrite(satellitePath))
        {
            satellitePe.SetResource(new Resource(resourceId, new byte[] { 0x02 }));
        }

        // Act
        using var pe = PortableExecutable.OpenRead(mainPath, openSatellites: true);
        var resource = pe.GetResource(resourceId);

        // Assert — satellite data should take precedence
        resource.Data.Should().Equal(new byte[] { 0x02 });
    }

    [Fact]
    public void I_can_get_the_MUI_info_and_FileVersionInfo_still_works()
    {
        // When a MUI resource is present, Windows redirects FileVersionInfo lookups
        // to a satellite .mui file. We create satellite files (copies of the dummy PE
        // with its version strings) so that Windows can find them through the redirect.
        if (!OperatingSystem.IsWindows())
            return;

        // Arrange
        using var dir = TempDir.Create();
        var mainPath = Path.Combine(dir.Path, "test.exe");
        File.Copy(Dummy.Program.Path, mainPath);

        // Create satellites for the current UI culture and its parents,
        // so Windows can find version strings regardless of the exact locale.
        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        var created = false;
        while (!string.IsNullOrEmpty(culture.Name))
        {
            var satDir = Path.Combine(dir.Path, culture.Name);
            Directory.CreateDirectory(satDir);
            File.Copy(Dummy.Program.Path, Path.Combine(satDir, "test.exe.mui"));
            culture = culture.Parent;
            created = true;
        }

        if (!created)
        {
            var satDir = Path.Combine(dir.Path, "en-US");
            Directory.CreateDirectory(satDir);
            File.Copy(Dummy.Program.Path, Path.Combine(satDir, "test.exe.mui"));
        }

        // Inject a MUI resource into the main file
        using (var pe = PortableExecutable.OpenWrite(mainPath))
        {
            pe.SetMuiInfo(
                new MuiInfo(
                    MuiFileType.LanguageNeutral,
                    checksum: new byte[16],
                    serviceChecksum: new byte[16],
                    mainResourceTypes: [ResourceType.Version],
                    fallbackResourceTypes: [],
                    language: null,
                    fallbackLanguage: null,
                    ultimateFallbackLanguage: "en"
                )
            );
        }

        // Act
        var versionInfo = FileVersionInfo.GetVersionInfo(mainPath);

        // Assert
        versionInfo.ProductName.Should().Be("TestProduct");
        versionInfo.FileDescription.Should().Be("TestDescription");
        versionInfo.CompanyName.Should().Be("TestCompany");
        versionInfo.FileVersion.Should().Be("1.2.3.4");
    }

    [Fact]
    public void I_can_get_more_resources_from_Notepad_with_satellites()
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
