using System;
using System.IO;

namespace Ressy.Utils
{
    // Allows to save a location in a stream and temporarily return back to it
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
            _stream.Position = Position;

            return Disposable.Create(() => _stream.Position = oldPosition);
        }
    }

    internal static class StreamCheckpointExtensions
    {
        public static StreamPortal CreatePortal(this Stream stream, long position) => new(stream, position);

        public static StreamPortal CreatePortal(this Stream stream) => new(stream, stream.Position);
    }
}