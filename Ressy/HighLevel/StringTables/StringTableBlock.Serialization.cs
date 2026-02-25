using System.IO;

namespace Ressy.HighLevel.StringTables;

public partial class StringTableBlock
{
    internal byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding);

        foreach (var str in Strings)
        {
            writer.Write((ushort)str.Length);

            foreach (var ch in str)
                writer.Write(ch);
        }

        return stream.ToArray();
    }
}
