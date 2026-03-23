#:package CliWrap

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CliWrap;

static string GetScriptDir([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;

var resourcesDir = GetScriptDir();
var rcFile = Path.Combine(resourcesDir, "Resources.rc");
var resFile = Path.Combine(resourcesDir, "Resources.res");

if (await InvokeWindres() || await InvokeRc())
    return;

if (File.Exists(resFile))
{
    Console.Error.WriteLine(
        "Warning: Could not compile resources: neither windres nor rc.exe was found or succeeded."
    );
    Console.Error.WriteLine(
        "Warning: "
            + (
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Install the Windows SDK (includes rc.exe):\n  winget install Microsoft.WindowsSDK.10.0.26100"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "Install mingw-w64 (includes windres):\n  sudo apt install mingw-w64"
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "Install mingw-w64 (includes windres):\n  brew install mingw-w64"
                : "Install the Windows SDK (rc.exe) or mingw-w64 (windres)."
            )
    );
    Console.Error.WriteLine("Warning: Using the existing Resources.res file.");
    return;
}

Console.Error.WriteLine(
    "Error: Could not compile resources: neither windres nor rc.exe was found or succeeded."
);
Console.Error.WriteLine(
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "Install the Windows SDK (includes rc.exe):\n  winget install Microsoft.WindowsSDK.10.0.26100"
    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "Install mingw-w64 (includes windres):\n  sudo apt install mingw-w64"
    : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        ? "Install mingw-w64 (includes windres):\n  brew install mingw-w64"
    : "Install the Windows SDK (rc.exe) or mingw-w64 (windres)."
);
Environment.Exit(1);

async Task<bool> InvokeWindres()
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
        var path = FindExecutable(candidate);
        if (path is null)
            continue;

        Console.WriteLine($"Using windres: {path}");

        var result = await Cli.Wrap(path)
            .WithArguments(["-i", rcFile, "-o", resFile, "-O", "res"])
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        if (result.ExitCode == 0)
            return true;

        Console.Error.WriteLine($"Warning: {candidate} failed with exit code {result.ExitCode}.");
    }

    return false;
}

async Task<bool> InvokeRc()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

    Console.WriteLine($"Using rc.exe: {rcExe}");

    var includeArgs = new List<string>();
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
            {
                includeArgs.Add("/I");
                includeArgs.Add(includePath);
            }
        }
    }

    var allArgs = new List<string>(includeArgs) { "/fo", resFile, rcFile };
    var result = await Cli.Wrap(rcExe)
        .WithArguments(allArgs)
        .WithValidation(CommandResultValidation.None)
        .ExecuteAsync();
    return result.ExitCode == 0;
}

string? FindExecutable(string name)
{
    var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(
        Path.PathSeparator,
        StringSplitOptions.RemoveEmptyEntries
    );

    var hasExtension = Path.GetExtension(name).Length > 0;
    var extensions =
        !hasExtension && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".exe;.cmd;.bat").Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries
            )
            : (string[])[""];

    foreach (var dir in paths)
    {
        foreach (var ext in extensions)
        {
            var fullPath = Path.Combine(dir, name + ext);
            if (File.Exists(fullPath))
                return fullPath;
        }
    }

    return null;
}
