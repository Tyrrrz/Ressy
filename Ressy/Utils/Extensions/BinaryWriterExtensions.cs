using System.IO;

namespace Ressy.Utils.Extensions;

internal static class BinaryWriterExtensions
{
    extension(BinaryWriter writer)
    {
        public void SkipPadding(int boundaryBits = 32)
        {
            while (writer.BaseStream.Position * 8 % boundaryBits != 0)
            {
                // Write a character so that it takes up either 1 or 2 bytes,
                // depending on the encoding of the stream.
                writer.Write('\0');
            }
        }
    }
}
