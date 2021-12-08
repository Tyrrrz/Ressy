using System.Drawing;
using System.IO;
using FluentAssertions;
using Ressy.HighLevel.Icons;
using Ressy.Tests.Fixtures;
using Ressy.Tests.Utils;
using Ressy.Tests.Utils.Extensions;
using Xunit;

namespace Ressy.Tests;

public class IconsSpecs : IClassFixture<DummyFixture>
{
    private readonly DummyFixture _dummy;

    public IconsSpecs(DummyFixture dummy) => _dummy = dummy;

    [Fact]
    public void User_can_add_an_icon()
    {
        // Arrange
        var iconFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, "TestData", "Icon.ico");

        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
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

    [Fact]
    public void User_can_remove_the_icon()
    {
        // Arrange
        var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

        // Act
        portableExecutable.RemoveIcon();

        // Assert
        portableExecutable.GetResourceIdentifiers().Should().NotContain(r =>
            r.Type.Code == ResourceType.IconGroup.Code ||
            r.Type.Code == ResourceType.Icon.Code
        );
    }
}