using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
    private static byte[] SerializeBlock(string[] strings)
    {
        if (strings.Length != BlockSize)
        {
            throw new ArgumentException(
                $"String table block must contain exactly {BlockSize} strings.",
                nameof(strings)
            );
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding);

        for (var i = 0; i < BlockSize; i++)
        {
            var str = strings[i];
            writer.Write((ushort)str.Length);

            foreach (var ch in str)
                writer.Write(ch);
        }

        return stream.ToArray();
    }

    internal IReadOnlyList<byte[]> Serialize()
    {
        if (Strings.Count == 0)
            return [];

        var blocks = new List<byte[]>();

        // Find the highest block ID needed
        var maxBlockId = Strings.Keys.Max(GetBlockId);

        // Generate all blocks from 1 to maxBlockId
        for (var blockId = 1; blockId <= maxBlockId; blockId++)
        {
            // Create a block filled with empty strings.
            // Null strings are not allowed in string table resources.
            var block = Enumerable.Repeat(string.Empty, BlockSize).ToArray();

            // Fill in strings for this block
            foreach (var (stringId, str) in Strings)
            {
                if (GetBlockId(stringId) != blockId)
                    continue;

                var blockIndex = GetBlockIndex(stringId);
                block[blockIndex] = str;
            }

            blocks.Add(SerializeBlock(block));
        }

        return blocks;
    }
}
