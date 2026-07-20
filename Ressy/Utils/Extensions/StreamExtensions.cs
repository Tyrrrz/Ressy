using System.IO;

namespace Ressy.Utils.Extensions;

internal static class StreamExtensions
{
    extension(Stream stream)
    {
        public MemoryStream ToMemoryStream()
        {
            if (stream is MemoryStream memoryStream)
                return memoryStream;

            var result = new MemoryStream();
            stream.CopyTo(result);
            result.Position = 0;

            return result;
        }
    }
}
