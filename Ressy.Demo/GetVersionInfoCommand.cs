using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using Ressy.Demo.Utils;
using Ressy.Versions;

namespace Ressy.Demo;

[Command("read version", Description = "Reads the version info resource from a PE file.")]
public partial class GetVersionInfoCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to read the version info resource from.")]
    public required string FilePath { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        using var portableExecutable = PortableExecutable.OpenRead(FilePath);
        var versionInfo = portableExecutable.GetVersionInfo();

        console.Output.WriteLine(
            JsonSerializer.Serialize(
                versionInfo,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new JsonStringEnumConverter(),
                        new VersionAttributesJsonConverter(),
                    },
                }
            )
        );

        return default;
    }
}
