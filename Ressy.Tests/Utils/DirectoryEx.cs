using System.IO;

namespace Ressy.Tests.Utils
{
    internal static class DirectoryEx
    {
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