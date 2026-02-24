using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Ressy.HighLevel.StringTables;
using Ressy.Tests.Utils;
using Xunit;

namespace Ressy.Tests;

public class StringTablesSpecs
{
    [Fact]
    public void I_can_get_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        portableExecutable.SetStringTable(
            new StringTableBuilder().SetString(1, "First").SetString(2, "Second").Build()
        );

        // Act
        var stringTable = portableExecutable.GetStringTable();

        // Assert
        stringTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("First");
        stringTable.Strings.Should().ContainKey(2).WhoseValue.Should().Be("Second");
    }

    [Fact]
    public void I_can_get_the_string_table_in_a_specific_language()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        var english = new Language(1033);

        portableExecutable.SetStringTable(
            new StringTableBuilder().SetString(1, "Hello").SetString(2, "Goodbye").Build(),
            english
        );

        var french = new Language(1036);

        portableExecutable.SetStringTable(
            new StringTableBuilder().SetString(1, "Bonjour").Build(),
            french
        );

        // Act
        var englishTable = portableExecutable.GetStringTable(english);
        var frenchTable = portableExecutable.GetStringTable(french);

        // Assert
        englishTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("Hello");
        englishTable.Strings.Should().ContainKey(2).WhoseValue.Should().Be("Goodbye");
        englishTable.Strings.Values.Should().NotContain("Bonjour");

        frenchTable.Strings.Should().ContainKey(1).WhoseValue.Should().Be("Bonjour");
        frenchTable.Strings.Values.Should().NotContain("Hello").And.NotContain("Goodbye");
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
    public void I_can_set_the_string_table_using_a_builder()
    {
        // Arrange
        var stringTable = new StringTableBuilder()
            .SetString(1, "Hello, World!")
            .SetString(2, "OldGoodbye")
            .Build();

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        portableExecutable.SetStringTable(stringTable);

        // Act
        portableExecutable.SetStringTable(b =>
        {
            b.SetString(2, "Goodbye, World!");
            b.SetString(3, "BrandNew");
        });

        // Assert
        portableExecutable.GetStringTable().GetString(1).Should().Be("Hello, World!");
        portableExecutable.GetStringTable().GetString(2).Should().Be("Goodbye, World!");
        portableExecutable.GetStringTable().GetString(3).Should().Be("BrandNew");
    }

    [Fact]
    public void I_can_set_the_string_table_in_a_specific_language()
    {
        // Arrange
        var englishStringTable = new StringTableBuilder()
            .SetString(1, "Hello")
            .SetString(2, "Goodbye")
            .Build();

        var frenchStringTable = new StringTableBuilder().SetString(1, "Bonjour").Build();

        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        var english = new Language(1033);
        portableExecutable.SetStringTable(englishStringTable, english);

        var french = new Language(1036);
        portableExecutable.SetStringTable(frenchStringTable, french);

        // Assert
        portableExecutable.GetStringTable(english).Should().BeEquivalentTo(englishStringTable);
        portableExecutable.GetStringTable(french).Should().BeEquivalentTo(frenchStringTable);
    }

    [Fact]
    public void I_can_modify_a_string_in_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetStringTable(
            new StringTableBuilder().SetString(1, "Hello, World!").SetString(2, "Untouched").Build()
        );

        // Act
        portableExecutable.SetStringTable(b => b.SetString(1, "Goodbye, World!"));

        // Assert
        portableExecutable.GetStringTable().GetString(1).Should().Be("Goodbye, World!");
        portableExecutable.GetStringTable().GetString(2).Should().Be("Untouched");
    }

    [Fact]
    public void I_can_remove_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetStringTable(
            new StringTableBuilder()
                .SetString(1, "Hello, World!")
                .SetString(2, "Goodbye, World!")
                .Build()
        );

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
