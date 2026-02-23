using System.IO;

namespace Ressy.Tests.Utils;

// Returns the path to a Windows PE file produced by Ressy.Tests.Dummy that carries
// the expected embedded resources (icons, version info, manifest).
// On Windows the native host .exe is used; on other platforms the managed .dll
// is used as it is also a valid PE file containing the same resources.
internal static class DummyPeFile
{
    public static string Path { get; } = Resolve();

    private static string Resolve()
    {
        var dllPath = typeof(Dummy.Program).Assembly.Location;
        var exePath = System.IO.Path.ChangeExtension(dllPath, "exe");
        return File.Exists(exePath) ? exePath : dllPath;
    }
}
