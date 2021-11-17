using System.IO;

namespace Ressy.Utils.Extensions
{
    internal static class StreamExtensions
    {
        public static void SeekTo32BitBoundary(this Stream stream)
        {
            var remainder = stream.Position % 4;
            if (remainder != 0)
                stream.Seek(4 - remainder, SeekOrigin.Current);
        }
    }
}