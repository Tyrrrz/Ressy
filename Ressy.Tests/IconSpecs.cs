using System;
using System.Drawing;
using System.IO;
using FluentAssertions;
using Ressy.HighLevel.Icons;
using Ressy.Tests.Utils;
using Ressy.Tests.Utils.Extensions;
using Xunit;

namespace Ressy.Tests;

public class IconSpecs
{
    [Fact]
    public void I_can_set_the_icon()
    {
        // Arrange
        var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveIcon();

        // Act
        portableExecutable.SetIcon(iconFilePath);

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .Contain([
                new ResourceIdentifier(
                    ResourceType.IconGroup,
                    ResourceName.FromCode(1),
                    Language.Neutral
                ),
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(1),
                    Language.Neutral
                ),
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(2),
                    Language.Neutral
                ),
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(3),
                    Language.Neutral
                ),
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(4),
                    Language.Neutral
                ),
                new ResourceIdentifier(
                    ResourceType.Icon,
                    ResourceName.FromCode(5),
                    Language.Neutral
                ),
            ]);

        if (OperatingSystem.IsWindows())
        {
            using var sourceIcon = new Icon(iconFilePath);
            using var actualIcon = Icon.ExtractAssociatedIcon(portableExecutable.FilePath);
            actualIcon.Should().NotBeNull();
            actualIcon?.ToBitmap().GetData().Should().Equal(sourceIcon.ToBitmap().GetData());
        }
    }

    [Fact]
    public void I_can_remove_the_icon()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveIcon();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r =>
                r.Type.Code == ResourceType.IconGroup.Code || r.Type.Code == ResourceType.Icon.Code
            );
    }
}
