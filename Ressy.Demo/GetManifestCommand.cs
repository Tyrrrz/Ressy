using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Ressy.HighLevel.Manifests;

namespace Ressy.Demo;

[Command("read manifest", Description = "Read the manifest resource from a PE file.")]
public class GetManifestCommand : ICommand
{
    [CommandOption("file", 'f', IsRequired = true, Description = "PE file to read the manifest resource from.")]
    public string FilePath { get; init; } = default!;

    public string FileName => Path.GetFileName(FilePath);

    public ValueTask ExecuteAsync(IConsole console)
    {
        var portableExecutable = new PortableExecutable(FilePath);
        var manifest = portableExecutable.GetManifest();

        console.Output.WriteLine($"Manifest resource in '{FileName}':");
        console.Output.WriteLine(manifest);

        return default;
    }
}