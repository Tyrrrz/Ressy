using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace Ressy.Demo;

[Command("list", Description = "Lists all available resources in a PE file.")]
public partial class ListResourcesCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to list resources from.")]
    public required string FilePath { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        using var portableExecutable = PortableExecutable.OpenRead(FilePath);

        foreach (var identifier in portableExecutable.GetResourceIdentifiers())
        {
            console.Output.WriteLine(
                $$"""
                {
                    "type": "{{identifier.Type}}",
                    "name": "{{identifier.Name}}",
                    "language": "{{identifier.Language}}"
                }
                """
            );
        }

        return default;
    }
}
