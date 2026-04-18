using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

[assembly: ExcludeFromCodeCoverage]

namespace Ressy.Tests.Dummy;

public static class Program
{
    public static string FilePath { get; } =
        Path.ChangeExtension(
            Assembly.GetExecutingAssembly().Location,
            // Fall back to DLL on non-Windows platforms, as the native apphost
            // there cannot contain Windows resource files.
            OperatingSystem.IsWindows()
                ? "exe"
                : "dll"
        );

    public static void Main() => Console.WriteLine("Hello world!");
}
