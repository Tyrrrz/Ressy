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
    }
}