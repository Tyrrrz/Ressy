using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Ressy.Manifests;

namespace Ressy.Demo;

[Command("read manifest", Description = "Reads the manifest resource from a PE file.")]
public class GetManifestCommand : ICommand
{
    [CommandOption("file", 'f', Description = "PE file to read the manifest resource from.")]
    public required string FilePath { get; init; }

    public string FileName => Path.GetFileName(FilePath);

    public ValueTask ExecuteAsync(IConsole console)
    {
        using var portableExecutable = PortableExecutable.OpenRead(FilePath);
        var manifest = portableExecutable.GetManifest();

        console.Output.WriteLine($"Manifest resource in '{FileName}':");
        console.Output.WriteLine(manifest);

        return default;
    }
}
