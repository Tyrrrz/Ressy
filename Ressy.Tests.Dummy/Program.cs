using System;
using System.IO;

namespace Ressy.Tests.Dummy;

public static class Program
{
    public static string Path { get; } = GetPath();

    public static void Main() => Console.WriteLine("Hello world!");

    private static string GetPath()
    {
        var dllPath = typeof(Program).Assembly.Location;
        var exePath = System.IO.Path.ChangeExtension(dllPath, "exe");
        return File.Exists(exePath) ? exePath : dllPath;
    }
}
