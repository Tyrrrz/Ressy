using System.Drawing;
using System.IO;
using FluentAssertions;
using Ressy.HighLevel.Icons;
using Ressy.Tests.Utils;
using Ressy.Tests.Utils.Extensions;
using Xunit;

namespace Ressy.Tests;

public class IconsSpecs
{
    [Fact]
    public void I_can_add_an_icon()
    {
        // Arrange
        var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveIcon();

        // Act
        portableExecutable.SetIcon(iconFilePath);

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().Contain(new[]
        {
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
            )
        });

        using var sourceIcon = new Icon(iconFilePath);
        using var actualIcon = Icon.ExtractAssociatedIcon(portableExecutable.FilePath);
        actualIcon.Should().NotBeNull();
        actualIcon?.ToBitmap().GetData().Should().Equal(sourceIcon.ToBitmap().GetData());
    }

    [Fact(Skip = "Takes a long time and doesn't seem to reproduce the issue when running on CI")]
    public void I_can_add_multiple_icons_in_quick_succession()
    {
        // https://github.com/Tyrrrz/Ressy/issues/4
        // For some reason, it's easiest to reproduce this with `SetIcon(...)` but
        // the underlying issue should probably affect all types of resource operations.

        // Arrange
        var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveIcon();

        // Act
        for (var i = 0; i < 100; i++)
            portableExecutable.SetIcon(iconFilePath);

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().Contain(r => r.Type.Code == ResourceType.Icon.Code);
    }

    [Fact]
    public void I_can_remove_the_icon()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveIcon();

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().NotContain(r =>
            r.Type.Code == ResourceType.IconGroup.Code ||
            r.Type.Code == ResourceType.Icon.Code
        );
    }
}