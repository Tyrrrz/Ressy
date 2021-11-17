using System;
using System.IO;

namespace Ressy.Utils.Extensions
{
    internal static class StreamExtensions
    {
        public static IDisposable JumpAndReturn(this Stream stream, long newPosition)
        {
            var oldPosition = stream.Position;
            stream.Position = newPosition;

            return Disposable.Create(() => stream.Position = oldPosition);
        }

        public static void SeekTo32BitBoundary(this Stream stream)
        {
            var remainder = stream.Position % 4;
            if (remainder != 0)
                stream.Seek(4 - remainder, SeekOrigin.Current);
        }
    }
}