using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Ressy.Tests.Utils;
using Ressy.Versions;
using Xunit;

namespace Ressy.Tests;

public class VersionInfoSpecs
{
    [Fact]
    public void I_can_get_the_version_info()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenRead(file.Path);

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
                                [VersionAttributeName.InternalName] = "TestProduct",
                                [VersionAttributeName.OriginalFilename] = "TestProduct.exe",
                            }
                        ),
                    ]
                )
            );
    }

    [Fact]
    public void I_can_set_the_version_info()
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
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);
        portableExecutable.RemoveVersionInfo();

        // Act
        portableExecutable.SetVersionInfo(versionInfo);
        portableExecutable.Stream.Flush();

        // Assert
        portableExecutable.GetVersionInfo().Should().BeEquivalentTo(versionInfo);

        // FileVersionInfo.GetVersionInfo() on non-Windows reads .NET assembly metadata
        // (via System.Reflection.Metadata) rather than Win32 version resources, so it
        // returns the original assembly attributes regardless of Win32 resource changes.
        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(file.Path)
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
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);

        // Act
        portableExecutable.SetVersionInfo(v =>
            v.SetFileVersion(new Version(4, 3, 2, 1))
                .SetFileOperatingSystem(
                    FileOperatingSystem.Windows32 | FileOperatingSystem.WindowsNT
                )
                .SetAttribute(VersionAttributeName.ProductName, "ProductTest")
                .SetAttribute(VersionAttributeName.CompanyName, "CompanyTest")
        );

        portableExecutable.Stream.Flush();

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
                                [VersionAttributeName.InternalName] = "TestProduct",
                                [VersionAttributeName.OriginalFilename] = "TestProduct.exe",
                            }
                        ),
                    ]
                )
            );

        // FileVersionInfo.GetVersionInfo() on non-Windows reads .NET assembly metadata
        // (via System.Reflection.Metadata) rather than Win32 version resources, so it
        // returns the original assembly attributes regardless of Win32 resource changes.
        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(file.Path)
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
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = PortableExecutable.OpenWrite(file.Path);

        // Act
        portableExecutable.RemoveVersionInfo();
        portableExecutable.Stream.Flush();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Code == ResourceType.Version.Code);

        portableExecutable.TryGetVersionInfo().Should().BeNull();

        // FileVersionInfo.GetVersionInfo() on non-Windows reads .NET assembly metadata
        // (via System.Reflection.Metadata) rather than Win32 version resources, so it
        // returns the original assembly attributes regardless of Win32 resource changes.
        if (OperatingSystem.IsWindows())
        {
            FileVersionInfo
                .GetVersionInfo(file.Path)
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
