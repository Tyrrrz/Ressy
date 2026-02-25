using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

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
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // Act
        portableExecutable.RemoveStringTable();

        // Assert
        portableExecutable
            .GetResourceIdentifiers()
            .Should()
            .NotContain(r => r.Type.Code == ResourceType.String.Code);

        portableExecutable.TryGetStringTable().Should().BeNull();
    }

    [Fact]
    public void I_can_read_a_resource_as_a_string_table_block()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);

        // String ID 1 is in block 1 (IDs 0-15)
        var blockId = StringTableBlock.GetBlockId(1);
        var identifier = portableExecutable
            .GetResourceIdentifiers()
            .First(r => r.Type.Code == ResourceType.String.Code && r.Name.Code == blockId);

        // Act
        var block = portableExecutable.GetResource(identifier).ReadAsStringTableBlock();

        // Assert
        block.BlockId.Should().Be(blockId);
        block.TryGetString(1).Should().Be("Hello, World!");
    }

    [Fact]
    public void I_can_construct_a_string_table_from_blocks()
    {
        // Arrange
        var block1Strings = new string[16];
        block1Strings[1] = "Hello, World!"; // string ID 1
        block1Strings[5] = "Goodbye, World!"; // string ID 5

        var block2Strings = new string[16];
        for (var i = 0; i < 16; i++)
            block2Strings[i] = string.Empty;
        block2Strings[2] = "Beep blop boop"; // string ID 18 (block 2, index 2)

        var blocks = new List<StringTableBlock>
        {
            new StringTableBlock(1, block1Strings),
            new StringTableBlock(2, block2Strings),
        };

        // Act
        var stringTable = StringTable.FromBlocks(blocks);

        // Assert
        stringTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("Hello, World!");
        stringTable.Strings.Should().ContainKey(5).WhoseValue.Should().Be("Goodbye, World!");
        stringTable.Strings.Should().ContainKey(18).WhoseValue.Should().Be("Beep blop boop");
    }

    [Fact]
    public void StringTableBlock_GetBlockId_returns_correct_block()
    {
        // String IDs 0-15 are in block 1
        StringTableBlock.GetBlockId(0).Should().Be(1);
        StringTableBlock.GetBlockId(15).Should().Be(1);

        // String IDs 16-31 are in block 2
        StringTableBlock.GetBlockId(16).Should().Be(2);
        StringTableBlock.GetBlockId(31).Should().Be(2);

        // String ID 18 is in block 2
        StringTableBlock.GetBlockId(18).Should().Be(2);
    }

    [Fact]
    public void StringTableBlock_GetBlockIndex_returns_correct_index()
    {
        // String ID 1 is at index 1 within block 1
        StringTableBlock.GetBlockIndex(1).Should().Be(1);

        // String ID 18 is at index 2 within block 2
        StringTableBlock.GetBlockIndex(18).Should().Be(2);

        // String ID 16 is at index 0 within block 2
        StringTableBlock.GetBlockIndex(16).Should().Be(0);
    }

    [Fact]
    public void StringTableBlock_TryGetString_returns_correct_value()
    {
        // Arrange
        var strings = new string[16];
        strings[1] = "Hello, World!";

        var block = new StringTableBlock(1, strings);

        // Act & Assert
        block.TryGetString(1).Should().Be("Hello, World!");
        block.TryGetString(0).Should().BeNull(); // empty string
        block.TryGetString(16).Should().BeNull(); // belongs to block 2
    }

    [Fact]
    public void StringTableBlock_GetString_throws_for_absent_string()
    {
        // Arrange
        var strings = new string[16];
        var block = new StringTableBlock(1, strings);

        // Act & Assert
        block.Invoking(b => b.GetString(0)).Should().Throw<InvalidOperationException>();
    }
}
