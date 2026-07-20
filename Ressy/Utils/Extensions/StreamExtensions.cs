using System.IO;

namespace Ressy.Utils.Extensions;

internal static class StreamExtensions
{
    extension(Stream stream)
    {
        public MemoryStream ToMemoryStream()
        {
            if (stream is MemoryStream asMemoryStream)
                return asMemoryStream;

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
