using System;
using System.IO;
using System.Reflection;

namespace Ressy.Tests.Utils;

internal partial class TempDir(string path) : IDisposable
{
    public string Path { get; } = path;

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException) { }
    }
}

internal partial class TempDir
{
    public static TempDir Create()
    {
        var basePath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Directory.GetCurrentDirectory(),
            "Temp"
        );

        Directory.CreateDirectory(basePath);

        var dirPath = System.IO.Path.Combine(basePath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        return new TempDir(dirPath);
    }
}
