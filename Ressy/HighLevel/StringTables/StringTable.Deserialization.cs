using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
    private static string[] DeserializeBlock(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding);

        var strings = new string[BlockSize];

        for (var i = 0; i < BlockSize; i++)
        {
            var length = reader.ReadUInt16();
            strings[i] = new string(reader.ReadChars(length));
        }

        return strings;
    }

    internal static StringTable Deserialize(IReadOnlyList<byte[]> blocks)
    {
        var strings = new Dictionary<int, string>();

        foreach (var (blockIndex, block) in blocks.Index())
        {
            var blockStrings = DeserializeBlock(block);

            foreach (var (i, blockString) in blockStrings.Index())
            {
                // Only include non-empty strings
                if (string.IsNullOrEmpty(blockString))
                    continue;

                var stringId = (blockIndex << 4) + i;
                strings[stringId] = blockString;
            }
        }

        return new StringTable(strings);
    }
}
