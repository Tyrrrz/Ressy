using System.IO;

namespace Ressy.Utils.Extensions
{
    internal static class BinaryWriterExtensions
    {
        public static void SkipPadding(this BinaryWriter writer, int bytes = 4)
        {
            var remainder = writer.BaseStream.Position % bytes;

            // Already on the boundary
            if (remainder == 0)
                return;

            for (var i = 0; i < remainder; i++)
                writer.Write((byte)0);
        }

        public static void WriteStringNullTerminated(this BinaryWriter writer, string value)
        {
            foreach (var c in value)
                writer.Write(c);

            writer.Write('\0');
        }
    }
}