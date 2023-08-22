using System.IO;

namespace Ressy.Utils.Extensions;

internal static class BinaryWriterExtensions
{
    public static void SkipPadding(this BinaryWriter writer, int boundaryBits = 32)
    {
        while (writer.BaseStream.Position * 8 % boundaryBits != 0)
        {
            // Write a character so that it takes up either 1 or 2 bytes,
            // depending on the encoding of the stream.
            writer.Write('\0');
        }
    }

    public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
    {
        foreach (var c in value)
            writer.Write(c);

        writer.Write('\0');
    }
}
