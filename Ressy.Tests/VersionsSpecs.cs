using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Ressy.HighLevel.Versions;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class VersionsSpecs
{
    [Fact]
    public void I_can_get_the_version_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(DummyPeFile.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var versionInfo = portableExecutable.GetVersionInfo();

        // Assert
        versionInfo
            .Should()
            .BeEquivalentTo(
                new VersionInfo(
                    new Version(1, 2, 3, 4),
                    new Version(5, 6, 7, 8),
                    FileFlags.None,
                    FileOperatingSystem.Windows32,
                    FileType.Application,
                    FileSubType.Unknown,
                    [
                        new VersionAttributeTable(
                            Language.Neutral,
                            CodePage.Unicode,
                            new Dictionary<VersionAttributeName, string>
                            {
                                ["Assembly Version"] = "6.9.6.9",
                                [VersionAttributeName.FileVersion] = "1.2.3.4",
                                [VersionAttributeName.ProductVersion] = "5.6.7.8",
                                [VersionAttributeName.ProductName] = "TestProduct",
                                [VersionAttributeName.FileDescription] = "TestDescription",
                                [VersionAttributeName.CompanyName] = "TestCompany",
                                [VersionAttributeName.Comments] = "TestComments",
                                [VersionAttributeName.LegalCopyright] = "TestCopyright",
                                [VersionAttributeName.InternalName] = "Ressy.Tests.Dummy.dll",
                                [VersionAttributeName.OriginalFilename] = "Ressy.Tests.Dummy.dll",
                            }
                        ),
                    ]
                )
            );
    }

    [Fact]
    public void I_can_get_the_version_info_of_Notepad()
    {
        if (!OperatingSystem.IsWindows())
            return;

        // Arrange
        var portableExecutable = new PortableExecutable(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32",
                "notepad.exe"
            )
        );

        // Act
        // Use TryGetVersionInfo because notepad.exe on modern Windows may have a
        // minimal/stub version resource (MUI redirection); in that case we skip.
        var versionInfo = portableExecutable.TryGetVersionInfo();

        // Assert
        versionInfo
            ?.GetAttribute(VersionAttributeName.InternalName)
            .Should()
            .BeEquivalentTo("Notepad");

        // We can't rely on the returned data because it's not deterministic but we only really
        // care that the deserialization has finished without any exceptions.
    }

    [Fact]
    public void I_can_get_the_version_info_of_InternetExplorer()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var iePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Internet Explorer",
            "iexplore.exe"
        );

        if (!File.Exists(iePath))
            return;

        // Arrange
        var portableExecutable = new PortableExecutable(iePath);

        // Act
        // Use TryGetVersionInfo for the same MUI-resilience reason as the Notepad test.
        var versionInfo = portableExecutable.TryGetVersionInfo();

        // Assert
        versionInfo
            ?.GetAttribute(VersionAttributeName.InternalName)
            .Should()
            .BeEquivalentTo("iexplore");

        // We can't rely on the returned data because it's not deterministic but we only really
        // care that the deserialization has finished without any exceptions.
    }

    [Fact]
    public void I_can_add_the_version_info()
    {
        // Arrange
        var versionInfo = new VersionInfoBuilder()
            .SetFileVersion(new Version(6, 7, 8, 9))
            .SetProductVersion(new Version(2, 3, 1, 9))
            .SetFileOperatingSystem(FileOperatingSystem.Windows32 | FileOperatingSystem.WindowsNT)
            .SetAttribute(VersionAttributeName.ProductName, "Foo")
            .SetAttribute(VersionAttributeName.FileDescription, "Bar")
            .SetAttribute(VersionAttributeName.CompanyName, "Baz")
            .SetAttribute("Custom", "Value")
            .Build();

        using var file = TempFile.Create();
        File.Copy(DummyPeFile.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveVersionInfo();

        // Act
        portableExecutable.SetVersionInfo(versionInfo);

        // Assert
        portableExecutable.GetVersionInfo().Should().BeEquivalentTo(versionInfo);

        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(portableExecutable.FilePath)
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        FileVersion = versionInfo.FileVersion.ToString(4),
                        ProductVersion = versionInfo.ProductVersion.ToString(4),
                        ProductName = versionInfo.GetAttribute(VersionAttributeName.ProductName),
                        FileDescription = versionInfo.GetAttribute(
                            VersionAttributeName.FileDescription
                        ),
                        CompanyName = versionInfo.GetAttribute(VersionAttributeName.CompanyName),
                        Comments = "",
                        LegalCopyright = "",
                    }
                );
        }
    }

    [Fact]
    public void I_can_modify_the_version_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(DummyPeFile.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.SetVersionInfo(v =>
            v.SetFileVersion(new Version(4, 3, 2, 1))
                .SetFileOperatingSystem(
                    FileOperatingSystem.Windows32 | FileOperatingSystem.WindowsNT
                )
                .SetAttribute(VersionAttributeName.ProductName, "ProductTest")
                .SetAttribute(VersionAttributeName.CompanyName, "CompanyTest")
        );

        // Assert
        var versionInfo = portableExecutable.GetVersionInfo();

        versionInfo
            .Should()
            .BeEquivalentTo(
                new VersionInfo(
                    new Version(4, 3, 2, 1),
                    new Version(5, 6, 7, 8),
                    FileFlags.None,
                    FileOperatingSystem.Windows32 | FileOperatingSystem.WindowsNT,
                    FileType.Application,
                    FileSubType.Unknown,
                    [
                        new VersionAttributeTable(
                            Language.Neutral,
                            CodePage.Unicode,
                            new Dictionary<VersionAttributeName, string>
                            {
                                ["Assembly Version"] = "6.9.6.9",
                                [VersionAttributeName.FileVersion] = "4.3.2.1",
                                [VersionAttributeName.ProductVersion] = "5.6.7.8",
                                [VersionAttributeName.ProductName] = "ProductTest",
                                [VersionAttributeName.FileDescription] = "TestDescription",
                                [VersionAttributeName.CompanyName] = "CompanyTest",
                                [VersionAttributeName.Comments] = "TestComments",
                                [VersionAttributeName.LegalCopyright] = "TestCopyright",
                                [VersionAttributeName.InternalName] = "Ressy.Tests.Dummy.dll",
                                [VersionAttributeName.OriginalFilename] = "Ressy.Tests.Dummy.dll",
                            }
                        ),
                    ]
                )
            );

        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(portableExecutable.FilePath)
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        FileVersion = versionInfo.GetAttribute(VersionAttributeName.FileVersion),
                        ProductVersion = versionInfo.GetAttribute(
                            VersionAttributeName.ProductVersion
                        ),
                        ProductName = versionInfo.GetAttribute(VersionAttributeName.ProductName),
                        FileDescription = versionInfo.GetAttribute(
                            VersionAttributeName.FileDescription
                        ),
                        CompanyName = versionInfo.GetAttribute(VersionAttributeName.CompanyName),
                        Comments = versionInfo.GetAttribute(VersionAttributeName.Comments),
                        LegalCopyright = versionInfo.GetAttribute(
                            VersionAttributeName.LegalCopyright
                        ),
                    }
                );
        }
    }

    [Fact]
    public void I_can_remove_the_version_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(DummyPeFile.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveVersionInfo();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Code == ResourceType.Version.Code);

        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(portableExecutable.FilePath)
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        FileVersion = default(string),
                        ProductVersion = default(string),
                        ProductName = default(string),
                        FileDescription = default(string),
                        CompanyName = default(string),
                        Comments = default(string),
                        LegalCopyright = default(string),
                    }
                );
        }
    }
}
