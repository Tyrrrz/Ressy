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

        portableExecutable.SetString(1, "First");
        portableExecutable.SetString(2, "Second");

        // Act
        var stringTable = portableExecutable.GetStringTable();

        // Assert
        stringTable
            .Should()
            .BeEquivalentTo(new Dictionary<int, string> { [1] = "First", [2] = "Second" });
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

        portableExecutable.SetString(1, "Hello", english);
        portableExecutable.SetString(1, "Bonjour", french);
        portableExecutable.SetString(2, "Goodbye", english);

        // Act
        var englishTable = portableExecutable.GetStringTable(english);
        var frenchTable = portableExecutable.GetStringTable(french);

        // Assert
        englishTable.Should().Contain(new KeyValuePair<int, string>(1, "Hello"));
        englishTable.Should().Contain(new KeyValuePair<int, string>(2, "Goodbye"));
        englishTable.Should().NotContain(kv => kv.Value == "Bonjour");

        frenchTable.Should().Contain(new KeyValuePair<int, string>(1, "Bonjour"));
        frenchTable.Should().NotContain(kv => kv.Value == "Hello" || kv.Value == "Goodbye");
    }

    [Fact]
    public void I_can_get_a_string_from_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetString(1, "Hello, World!");

        // Act
        var result = portableExecutable.GetString(1);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void I_can_get_a_string_from_the_string_table_in_a_specific_language()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        var english = new Language(1033);
        var french = new Language(1036);

        portableExecutable.SetString(1, "Hello", english);
        portableExecutable.SetString(1, "Bonjour", french);

        // Act
        var englishString = portableExecutable.GetString(1, english);
        var frenchString = portableExecutable.GetString(1, french);

        // Assert
        englishString.Should().Be("Hello");
        frenchString.Should().Be("Bonjour");
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
            new Dictionary<int, string>
            {
                [1] = "First",
                [2] = "Second",
                [100] = "OneHundred",
            }
        );

        // Assert
        portableExecutable
            .GetStringTable()
            .Should()
            .BeEquivalentTo(
                new Dictionary<int, string>
                {
                    [1] = "First",
                    [2] = "Second",
                    [100] = "OneHundred",
                }
            );
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
            new Dictionary<int, string> { [1] = "Hello", [2] = "Goodbye" },
            english
        );
        portableExecutable.SetStringTable(new Dictionary<int, string> { [1] = "Bonjour" }, french);

        // Assert
        portableExecutable
            .GetStringTable(english)
            .Should()
            .ContainKey(1)
            .WhoseValue.Should()
            .Be("Hello");
        portableExecutable
            .GetStringTable(french)
            .Should()
            .ContainKey(1)
            .WhoseValue.Should()
            .Be("Bonjour");
    }

    [Fact]
    public void I_can_add_a_string_to_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        portableExecutable.SetString(1, "Hello, World!");

        // Assert
        portableExecutable.GetString(1).Should().Be("Hello, World!");
    }

    [Fact]
    public void I_can_add_a_string_to_the_string_table_in_a_specific_language()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        var english = new Language(1033);
        var french = new Language(1036);

        // Act
        portableExecutable.SetString(1, "Hello", english);
        portableExecutable.SetString(1, "Bonjour", french);

        // Assert
        portableExecutable.GetString(1, english).Should().Be("Hello");
        portableExecutable.GetString(1, french).Should().Be("Bonjour");
    }

    [Fact]
    public void I_can_add_multiple_strings_to_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        portableExecutable.SetString(1, "First");
        portableExecutable.SetString(2, "Second");
        portableExecutable.SetString(100, "OneHundred");
        portableExecutable.SetString(1000, "OneThousand");

        // Assert
        portableExecutable
            .GetStringTable()
            .Should()
            .BeEquivalentTo(
                new Dictionary<int, string>
                {
                    [1] = "First",
                    [2] = "Second",
                    [100] = "OneHundred",
                    [1000] = "OneThousand",
                }
            );
    }

    [Fact]
    public void I_can_modify_a_string_in_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetString(1, "Hello, World!");

        // Act
        portableExecutable.SetString(1, "Goodbye, World!");

        // Assert
        portableExecutable.GetString(1).Should().Be("Goodbye, World!");
    }

    [Fact]
    public void I_can_remove_the_string_table()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.SetString(1, "Hello, World!");

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
