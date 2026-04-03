using System.Globalization;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace Ressy.Demo;

[Command("read", Description = "Reads a specific resource from a PE file.")]
public partial class GetResourceDataCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to read the resource from.")]
    public required string FilePath { get; set; }

    [CommandOption("type", 't', Description = "Type of the resource to read.")]
    public required string Type { get; set; }

    [CommandOption("name", 'n', Description = "Name of the resource to read.")]
    public required string Name { get; set; }

    [CommandOption("lang", 'l', Description = "Language of the resource to read.")]
    public int Language { get; set; } = Ressy.Language.Neutral.Id;

    public ValueTask ExecuteAsync(IConsole console)
    {
        using var portableExecutable = PortableExecutable.OpenRead(FilePath);

        var type = int.TryParse(Type, CultureInfo.InvariantCulture, out var typeCode)
            ? ResourceType.FromCode(typeCode)
            : ResourceType.FromString(Type);

        var name = int.TryParse(Name, CultureInfo.InvariantCulture, out var nameCode)
            ? ResourceName.FromCode(nameCode)
            : ResourceName.FromString(Name);

        var language = new Language(Language);

        var resource = portableExecutable.GetResource(new ResourceIdentifier(type, name, language));
        console.Output.BaseStream.Write(resource.Data);

        return default;
    }
}
