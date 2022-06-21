using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ressy.HighLevel.Versions;

namespace Ressy.Demo;

[Command("read version", Description = "Read the version info resource from a PE file.")]
public class GetVersionInfoCommand : ICommand
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        Converters = { new StringEnumConverter() }
    };

    [CommandOption(
        "file",
        'f',
        IsRequired = true,
        Description = "PE file to read the version info resource from."
    )]
    public string FilePath { get; init; } = default!;

    public ValueTask ExecuteAsync(IConsole console)
    {
        var portableExecutable = new PortableExecutable(FilePath);
        var versionInfo = portableExecutable.GetVersionInfo();

        console.Output.WriteLine(JsonConvert.SerializeObject(versionInfo, JsonSerializerSettings));

        return default;
    }
}