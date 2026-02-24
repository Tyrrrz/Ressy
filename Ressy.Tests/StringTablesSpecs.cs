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
            new StringTable(new Dictionary<int, string> { [1] = "First", [2] = "Second" })
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
        var french = new Language(1036);

        portableExecutable.SetStringTable(
            new StringTable(new Dictionary<int, string> { [1] = "Hello", [2] = "Goodbye" }),
            english
        );
        portableExecutable.SetStringTable(
            new StringTable(new Dictionary<int, string> { [1] = "Bonjour" }),
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
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        portableExecutable.SetStringTable(
            new StringTable(
                new Dictionary<int, string>
                {
                    [1] = "First",
                    [2] = "Second",
                    [100] = "OneHundred",
                }
            )
        );

        // Assert
        portableExecutable
            .GetStringTable()
            .Should()
            .BeEquivalentTo(
                new StringTable(
                    new Dictionary<int, string>
                    {
                        [1] = "First",
                        [2] = "Second",
                        [100] = "OneHundred",
                    }
                )
            );
    }

    [Fact]
    public void I_can_set_the_string_table_using_a_builder()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        portableExecutable.SetStringTable(
            new StringTable(
                new Dictionary<int, string> { [1] = "Hello, World!", [2] = "OldGoodbye" }
            )
        );

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
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        var english = new Language(1033);
        var french = new Language(1036);

        // Act
        portableExecutable.SetStringTable(
            new StringTable(new Dictionary<int, string> { [1] = "Hello", [2] = "Goodbye" }),
            english
        );
        portableExecutable.SetStringTable(
            new StringTable(new Dictionary<int, string> { [1] = "Bonjour" }),
            french
        );

        // Assert
        portableExecutable.GetStringTable(english).GetString(1).Should().Be("Hello");
        portableExecutable.GetStringTable(french).GetString(1).Should().Be("Bonjour");
    }

    [Fact]
    public void I_can_modify_a_string_in_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetStringTable(
            new StringTable(
                new Dictionary<int, string> { [1] = "Hello, World!", [2] = "Untouched" }
            )
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
            new StringTable(new Dictionary<int, string> { [1] = "Hello, World!" })
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
