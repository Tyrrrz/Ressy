using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
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
            var blockStrings = Enumerable.Repeat(string.Empty, BlockSize).ToArray();

            // Fill in strings for this block
            foreach (var (stringId, str) in Strings)
            {
                if (GetBlockId(stringId) != blockId)
                    continue;

                blockStrings[GetBlockIndex(stringId)] = str;
            }

            blocks.Add(new StringTableBlock(blockId, blockStrings).Serialize());
        }

        return blocks;
    }
}
