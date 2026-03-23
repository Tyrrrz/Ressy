#:package CliWrap
#:package CliFx

using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliWrap;

return await new CliApplicationBuilder().AddCommand<CompileResourcesCommand>().Build().RunAsync();

[Command]
public class CompileResourcesCommand : ICommand
{
    [CommandOption("input", 'i', IsRequired = true)]
    public required string InputFilePath { get; set; }

    [CommandOption("output", 'o', IsRequired = true)]
    public required string OutputFilePath { get; set; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        if (
            await InvokeWindresAsync(console, cancellationToken)
            || await InvokeRcAsync(console, cancellationToken)
        )
        {
            return;
        }

        throw new CommandException(
            "Could not compile resources: neither windres nor rc.exe was found or succeeded.\n"
                + (
                    OperatingSystem.IsWindows()
                        ? "Install the Windows SDK (includes rc.exe):\n  winget install Microsoft.WindowsSDK.10.0.26100"
                    : OperatingSystem.IsLinux()
                        ? "Install mingw-w64 (includes windres):\n  sudo apt install mingw-w64"
                    : OperatingSystem.IsMacOS()
                        ? "Install mingw-w64 (includes windres):\n  brew install mingw-w64"
                    : "Install the Windows SDK (rc.exe) or mingw-w64 (windres)."
                )
        );
    }

    private async Task<bool> InvokeWindresAsync(IConsole console, CancellationToken cancellationToken)
    {
        string[] candidates =
        [
            "x86_64-w64-mingw32-windres",
            "i686-w64-mingw32-windres",
            "windres",
            "windres.exe",
        ];

        foreach (var candidate in candidates)
        {
            CommandResult result;
            try
            {
                result = await Cli.Wrap(candidate)
                    .WithArguments(["-i", InputFilePath, "-o", OutputFilePath, "-O", "res"])
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                continue;
            }

            if (result.ExitCode == 0)
            {
                await console.Output.WriteLineAsync($"Using windres: {candidate}");
                return true;
            }

            await console.Error.WriteLineAsync(
                $"Warning: {candidate} failed with exit code {result.ExitCode}."
            );
        }

        return false;
    }

    private async Task<bool> InvokeRcAsync(IConsole console, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var windowsKitsPath = @"C:\Program Files (x86)\Windows Kits";
        if (!Directory.Exists(windowsKitsPath))
            return false;

        var rcExe = Directory
            .EnumerateFiles(windowsKitsPath, "rc.exe", SearchOption.AllDirectories)
            .Where(p => p.Contains(@"\x64\rc.exe") || p.Contains(@"\x86\rc.exe"))
            .Where(p => Regex.IsMatch(p, @"\\bin\\[\d.]+\\"))
            .OrderByDescending(p =>
            {
                var m = Regex.Match(p, @"\\bin\\([\d.]+)\\");
                return m.Success ? Version.Parse(m.Groups[1].Value) : new Version(0, 0);
            })
            .ThenBy(p => p.Contains(@"\x86\") ? 1 : 0)
            .FirstOrDefault();

        if (rcExe is null)
            return false;

        await console.Output.WriteLineAsync($"Using rc.exe: {rcExe}");

        var result = await Cli.Wrap(rcExe)
            .WithArguments(args =>
            {
                var versionMatch = Regex.Match(rcExe, @"\\bin\\([\d.]+)\\");
                if (versionMatch.Success)
                {
                    var sdkVersion = versionMatch.Groups[1].Value;
                    var sdkRoot = Path.GetFullPath(
                        Path.Combine(Path.GetDirectoryName(rcExe)!, "..", "..", "..")
                    );
                    foreach (var subdir in new[] { "um", "shared" })
                    {
                        var includePath = Path.Combine(sdkRoot, "Include", sdkVersion, subdir);
                        if (Directory.Exists(includePath))
                            args.Add("/I").Add(includePath);
                    }
                }

                args.Add("/fo").Add(OutputFilePath).Add(InputFilePath);
            })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(cancellationToken);

        return result.ExitCode == 0;
    }
}
