using System.IO;
using FluentAssertions;
using Ressy.HighLevel.StringTables;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class StringTableSpecs
{
    [Fact]
    public void I_can_get_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var stringTable = portableExecutable.GetStringTable();

        // Assert
        stringTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("Hello, World!");
        stringTable.Strings.Should().ContainKey(5).WhoseValue.Should().Be("Goodbye, World!");
        stringTable.Strings.Should().ContainKey(18).WhoseValue.Should().Be("Beep blop boop");
    }

    [Fact]
    public void I_can_get_the_string_table_in_a_specific_language()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);

        // Act
        var stringTable = portableExecutable.GetStringTable(new Language(1036));

        // Assert
        stringTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("Bonjour, le monde !");
        stringTable.Strings.Should().ContainKey(5).WhoseValue.Should().Be("Au revoir, le monde !");
        stringTable.Strings.Should().ContainKey(18).WhoseValue.Should().Be("Bip blop boup");
    }

    [Fact]
    public void I_can_set_the_string_table()
    {
        // Arrange
        var stringTable = new StringTableBuilder()
            .SetString(1, "First")
            .SetString(2, "Second")
            .SetString(100, "OneHundred")
            .Build();

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        portableExecutable.SetStringTable(stringTable);

        // Assert
        portableExecutable.GetStringTable().Should().BeEquivalentTo(stringTable);
    }

    [Fact]
    public void I_can_set_the_string_table_in_a_specific_language()
    {
        // Arrange
        var stringTable = new StringTableBuilder()
            .SetString(1, "Premier")
            .SetString(2, "Deuxième")
            .SetString(100, "Cent")
            .Build();

        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        portableExecutable.SetStringTable(stringTable, new Language(1036));

        // Assert
        portableExecutable.GetStringTable(new Language(1036)).Should().BeEquivalentTo(stringTable);
    }

    [Fact]
    public void I_can_modify_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.SetStringTable(b => b.SetString(1, "Foo bar").SetString(3, "Baz qux"));

        // Assert
        portableExecutable
            .GetStringTable()
            .Strings.Should()
            .ContainKey(1)
            .WhoseValue.Should()
            .Be("Foo bar");
        portableExecutable
            .GetStringTable()
            .Strings.Should()
            .ContainKey(3)
            .WhoseValue.Should()
            .Be("Baz qux");
        portableExecutable
            .GetStringTable()
            .Strings.Should()
            .ContainKey(5)
            .WhoseValue.Should()
            .Be("Goodbye, World!");
        portableExecutable
            .GetStringTable()
            .Strings.Should()
            .ContainKey(18)
            .WhoseValue.Should()
            .Be("Beep blop boop");
    }

    [Fact]
    public void I_can_remove_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Dummy.Program.Path, file.Path);

        using var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveStringTable();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Code == ResourceType.String.Code);

        portableExecutable.TryGetStringTable().Should().BeNull();
    }
}
