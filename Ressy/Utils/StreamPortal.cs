using System;
using System.IO;

namespace Ressy.Utils;

// Allows to temporarily go to a location in a stream and then return back to the original position
internal class StreamPortal(Stream stream, long position)
{
    public long Position { get; } = position;

    public IDisposable Jump()
    {
        var oldPosition = stream.Position;
        stream.Seek(Position, SeekOrigin.Begin);

        return Disposable.Create(() => stream.Seek(oldPosition, SeekOrigin.Begin));
    }
}

internal static class StreamPortalExtensions
{
    extension(Stream stream)
    {
        public StreamPortal CreatePortal(long position) => new(stream, position);

        public StreamPortal CreatePortal() => stream.CreatePortal(stream.Position);
    }
}
