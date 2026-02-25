using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Ressy.StringTables;

namespace Ressy.Demo;

[Command("read strings", Description = "Reads string table resource blocks from a PE file.")]
public class GetStringTableCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to read string table resource blocks from.")]
    public required string FilePath { get; init; }

    [CommandOption("lang", 'l', Description = "Language of the string to read.")]
    public int Language { get; init; } = Ressy.Language.UINeutral.Id;

    public ValueTask ExecuteAsync(IConsole console)
    {
        var portableExecutable = PortableExecutable.OpenRead(FilePath);
        var stringTable = portableExecutable.GetStringTable(new Language(Language));

        foreach (var (id, value) in stringTable.Strings)
        {
            console.Output.WriteLine(
                $$"""
                {
                    "id": {{id}},
                    "value": "{{value}}"
                }
                """
            );
        }

        return default;
    }
}
