using System;
using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Contains strings loaded from string table resource blocks, keyed by their IDs.
/// </summary>
/// <remarks>
/// String table data is stored in a portable executable file as a series of resource blocks,
/// each containing 16 strings (see <see cref="StringTableBlock" />).
/// This class provides a unified view over all blocks.
/// </remarks>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public partial class StringTable(IReadOnlyDictionary<int, string> strings)
{
    /// <summary>
    /// Contains the string entries, keyed by their IDs.
    /// </summary>
    public IReadOnlyDictionary<int, string> Strings { get; } = strings;

    /// <summary>
    /// Gets the string with the specified ID.
    /// Returns <c>null</c> if the string doesn't exist.
    /// </summary>
    public string? TryGetString(int stringId) =>
        Strings.TryGetValue(stringId, out var value) ? value : null;

    /// <summary>
    /// Gets the string with the specified ID.
    /// </summary>
    public string GetString(int stringId) =>
        TryGetString(stringId)
        ?? throw new InvalidOperationException(
            $"String with ID '{stringId}' does not exist in the string table."
        );
}

public partial class StringTable
{
    /// <summary>
    /// Creates a new <see cref="StringTable" /> from a collection of string table resource blocks.
    /// </summary>
    /// <remarks>
    /// Non-empty strings from all blocks are merged into a unified view keyed by their IDs.
    /// Blocks with duplicate IDs are merged in the order they appear in the input sequence.
    /// </remarks>
    public static StringTable FromBlocks(IReadOnlyList<StringTableBlock> blocks)
    {
        var strings = new Dictionary<int, string>();

        foreach (var block in blocks)
        {
            foreach (var (i, str) in block.Strings.Index())
            {
                if (!string.IsNullOrEmpty(str))
                    strings[(block.BlockId - 1) * StringTableBlock.BlockSize + i] = str;
            }
        }

        return new StringTable(strings);
    }
}
