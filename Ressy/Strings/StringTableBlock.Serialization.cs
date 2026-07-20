using System.IO;

namespace Ressy.Strings;

public partial class StringTableBlock
{
    internal void Serialize(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding);

        foreach (var str in Strings)
        {
            writer.Write((ushort)str.Length);

            foreach (var ch in str)
                writer.Write(ch);
        }
    }

    internal byte[] Serialize()
    {
        using var stream = new MemoryStream();
        Serialize(stream);

        return stream.ToArray();
    }
}
