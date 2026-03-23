using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var resourcesDir = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
var rcFile = Path.Combine(resourcesDir, "Resources.rc");
var resFile = Path.Combine(resourcesDir, "Resources.res");

if (InvokeWindres() || InvokeRc())
    return;

if (File.Exists(resFile))
{
    Console.Error.WriteLine(
        "Warning: Could not compile resources: neither windres nor rc.exe was found or succeeded."
    );
    WriteInstallHint(isError: false);
    Console.Error.WriteLine("Warning: Using the existing Resources.res file.");
    return;
}

Console.Error.WriteLine(
    "Error: Could not compile resources: neither windres nor rc.exe was found or succeeded."
);
WriteInstallHint(isError: true);
Environment.Exit(1);

bool InvokeWindres()
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

        var exitCode = RunProcess(path, "-i", rcFile, "-o", resFile, "-O", "res");
        if (exitCode == 0)
            return true;

        Console.Error.WriteLine($"Warning: {candidate} failed with exit code {exitCode}.");
    }

    return false;
}

bool InvokeRc()
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
    return RunProcess(rcExe, [.. allArgs]) == 0;
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

int RunProcess(string executable, params string[] arguments)
{
    var psi = new ProcessStartInfo(executable) { UseShellExecute = false };
    foreach (var arg in arguments)
        psi.ArgumentList.Add(arg);

    using var process =
        Process.Start(psi)
        ?? throw new InvalidOperationException($"Failed to start '{executable}'.");
    process.WaitForExit();
    return process.ExitCode;
}

void WriteInstallHint(bool isError)
{
    var prefix = isError ? "" : "Warning: ";

    string hint =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Install the Windows SDK (includes rc.exe):\n  winget install Microsoft.WindowsSDK.10.0.26100"
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? "Install mingw-w64 (includes windres):\n  sudo apt install mingw-w64"
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? "Install mingw-w64 (includes windres):\n  brew install mingw-w64"
        : "Install the Windows SDK (rc.exe) or mingw-w64 (windres).";

    Console.Error.WriteLine(prefix + hint);
}
