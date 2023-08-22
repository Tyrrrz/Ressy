using System;
using System.IO;

namespace Ressy.Utils;

// Allows to temporarily go to a location in a stream and then return back to the original position
internal class StreamPortal
{
    private readonly Stream _stream;

    public long Position { get; }

    public StreamPortal(Stream stream, long position)
    {
        _stream = stream;
        Position = position;
    }

    public IDisposable Jump()
    {
        var oldPosition = _stream.Position;
        _stream.Seek(Position, SeekOrigin.Begin);

        return Disposable.Create(() => _stream.Seek(oldPosition, SeekOrigin.Begin));
    }
}

internal static class StreamPortalExtensions
{
    public static StreamPortal CreatePortal(this Stream stream, long position) =>
        new(stream, position);

    public static StreamPortal CreatePortal(this Stream stream) =>
        stream.CreatePortal(stream.Position);
}
