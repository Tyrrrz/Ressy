using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Ressy.Abstractions.Versions;

namespace Ressy.Demo
{
    [Command("read version", Description = "Reads the version info resource from a PE file.")]
    public class GetVersionInfoCommand : ICommand
    {
        [CommandOption("file", 'f', IsRequired = true, Description = "PE file to read the version info resource from.")]
        public string FilePath { get; init; } = default!;

        public string FileName => Path.GetFileName(FilePath);

        public ValueTask ExecuteAsync(IConsole console)
        {
            var portableExecutable = new PortableExecutable(FilePath);
            var versionInfo = portableExecutable.GetVersionInfo();

            console.Output.WriteLine($"Version info resource in '{FileName}':");
            console.Output.WriteLine(JsonSerializer.Serialize(versionInfo));

            return default;
        }
    }
}