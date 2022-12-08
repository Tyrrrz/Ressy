using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Ressy.Demo;

[Command("list", Description = "List all available resources in a PE file.")]
public class ListResourcesCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to list resources from.")]
    public required string FilePath { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var portableExecutable = new PortableExecutable(FilePath);

        foreach (var identifier in portableExecutable.GetResourceIdentifiers())
        {
            console.Output.WriteLine(
                "{ " +
                $"Type: {identifier.Type}, " +
                $"Name: {identifier.Name}, " +
                $"Language: {identifier.Language}" +
                "}"
            );
        }

        return default;
    }
}