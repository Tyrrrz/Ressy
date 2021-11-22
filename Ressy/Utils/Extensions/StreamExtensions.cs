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

            // Already on the boundary
            if (remainder == 0)
                return;

            var padding = Math.Min(4 - remainder, stream.Length - stream.Position);
            stream.Seek(padding, SeekOrigin.Current);
        }
    }
}