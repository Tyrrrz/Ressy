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
    public void I_can_set_a_string()
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
    public void I_can_set_multiple_strings()
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
    public void I_can_overwrite_a_string()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        portableExecutable.SetString(1, "Hello, World!");

        // Act
        portableExecutable.SetString(1, "Goodbye, World!");

        // Assert
        portableExecutable.GetString(1).Should().Be("Goodbye, World!");
    }

    [Fact]
    public void I_can_try_to_get_a_string_that_does_not_exist_and_receive_null_instead()
    {
        // Arrange
        using var file = TempFile.Create();
        File.Copy(Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe"), file.Path);

        var portableExecutable = new PortableExecutable(file.Path);
        portableExecutable.RemoveStringTable();

        // Act
        var result = portableExecutable.TryGetString(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void I_can_set_strings_for_a_specific_language()
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

        portableExecutable.GetStringTable().Should().BeEmpty();
    }
}
