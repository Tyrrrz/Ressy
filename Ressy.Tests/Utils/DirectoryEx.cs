using System.IO;

namespace Ressy.Tests.Utils
{
    internal static class DirectoryEx
    {
        public static string ExecutingDirectoryPath { get; } =
            Path.GetDirectoryName(typeof(DirectoryEx).Assembly.Location) ??
            Directory.GetCurrentDirectory();

        public static void DeleteIfExists(string dirPath, bool recursive = true)
        {
            try
            {
                Directory.Delete(dirPath, recursive);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }
}