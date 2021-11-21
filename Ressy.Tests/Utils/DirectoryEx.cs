using System.IO;

namespace Ressy.Tests.Utils
{
    internal static class DirectoryEx
    {
        public static string ExecutingDirectoryPath { get; } =
            Path.GetDirectoryName(typeof(DirectoryEx).Assembly.Location) ??
            Directory.GetCurrentDirectory();
    }
}