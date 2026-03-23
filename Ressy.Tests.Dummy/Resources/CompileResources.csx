#!/usr/bin/dotnet --
#:package CliWrap
#:package CliFx

using System.ComponentModel;
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
            OperatingSystem.IsWindows()
                ? """
                Could not compile resources: neither windres nor rc.exe was found or succeeded.
                Install the Windows SDK (includes rc.exe):
                  winget install Microsoft.WindowsSDK.10.0.26100
                """
            : OperatingSystem.IsLinux()
                ? """
                Could not compile resources: neither windres nor rc.exe was found or succeeded.
                Install mingw-w64 (includes windres):
                  sudo apt install mingw-w64
                """
            : OperatingSystem.IsMacOS()
                ? """
                Could not compile resources: neither windres nor rc.exe was found or succeeded.
                Install mingw-w64 (includes windres):
                  brew install mingw-w64
                """
            : """
            Could not compile resources: neither windres nor rc.exe was found or succeeded.
            Install the Windows SDK (rc.exe) or mingw-w64 (windres).
            """
        );
    }

    private async Task<bool> InvokeWindresAsync(
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        var candidates = new string[]
        {
            "x86_64-w64-mingw32-windres",
            "i686-w64-mingw32-windres",
            "windres",
            "windres.exe",
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var result = await Cli.Wrap(candidate)
                    .WithArguments(["-i", InputFilePath, "-o", OutputFilePath, "-O", "res"])
                    .WithStandardOutputPipe(
                        PipeTarget.ToDelegate(async line =>
                            await console.Output.WriteLineAsync(line)
                        )
                    )
                    .WithStandardErrorPipe(
                        PipeTarget.ToDelegate(async line =>
                            await console.Error.WriteLineAsync(line)
                        )
                    )
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync(cancellationToken);

                if (result.ExitCode == 0)
                {
                    await console.Output.WriteLineAsync($"Using windres: {candidate}");
                    return true;
                }

                await console.Error.WriteLineAsync(
                    $"Warning: {candidate} failed with exit code {result.ExitCode}."
                );
            }
            // Target executable not found
            catch (Win32Exception)
            {
                continue;
            }
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

        var rcFilePath = Directory
            .EnumerateFiles(windowsKitsPath, "rc.exe", SearchOption.AllDirectories)
            .Where(p =>
                p.Contains(@"\x64\rc.exe", StringComparison.OrdinalIgnoreCase)
                || p.Contains(@"\x86\rc.exe", StringComparison.OrdinalIgnoreCase)
            )
            .Where(p => Regex.IsMatch(p, @"\\bin\\[\d.]+\\"))
            .OrderByDescending(p =>
            {
                var match = Regex.Match(p, @"\\bin\\([\d.]+)\\");
                return match.Success ? Version.Parse(match.Groups[1].Value) : new Version(0, 0);
            })
            .ThenBy(p => p.Contains(@"\x86\", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (rcFilePath is null)
            return false;

        await console.Output.WriteLineAsync($"Using rc.exe: {rcFilePath}");

        var result = await Cli.Wrap(rcFilePath)
            .WithArguments(args =>
            {
                var versionMatch = Regex.Match(rcFilePath, @"\\bin\\([\d.]+)\\");
                if (versionMatch.Success)
                {
                    var sdkVersion = versionMatch.Groups[1].Value;
                    var sdkRoot = Path.GetFullPath(
                        Path.Combine(Path.GetDirectoryName(rcFilePath)!, "..", "..", "..")
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
            .WithStandardOutputPipe(
                PipeTarget.ToDelegate(async line => await console.Output.WriteLineAsync(line))
            )
            .WithStandardErrorPipe(
                PipeTarget.ToDelegate(async line => await console.Error.WriteLineAsync(line))
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(cancellationToken);

        return result.ExitCode == 0;
    }
}
