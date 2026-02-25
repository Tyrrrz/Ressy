using System.Collections.Generic;
using System.Text;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Represents a single string table resource block containing exactly 16 strings.
/// </summary>
/// <remarks>
/// String table data in a portable executable file is organized into blocks of 16 strings each.
/// Each block is stored as a separate resource of type <see cref="ResourceType.String" />,
/// identified by the block ID as its ordinal resource name.
/// </remarks>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public partial class StringTableBlock(int blockId, IReadOnlyList<string> strings)
{
    /// <summary>
    /// Block ID (1-based).
    /// Corresponds to the ordinal name of the underlying resource of type
    /// <see cref="ResourceType.String" />.
    /// </summary>
    /// <remarks>
    /// Strings in this block have IDs in the range
    /// <c>[(<see cref="BlockId" /> - 1) * 16, <see cref="BlockId" /> * 16 - 1]</c>.
    /// </remarks>
    public int BlockId { get; } = blockId;

    /// <summary>
    /// The 16 strings contained in this block, indexed by their position within the block (0-15).
    /// Empty strings indicate that no string is assigned the corresponding ID.
    /// </summary>
    /// <remarks>
    /// The global string ID for index <c>i</c> is <c>(<see cref="BlockId" /> - 1) * 16 + i</c>.
    /// </remarks>
    public IReadOnlyList<string> Strings { get; } = strings;
}

public partial class StringTableBlock
{
    internal const int BlockSize = 16;

    internal static int GetBlockId(int stringId) => (stringId >> 4) + 1;

    internal static int GetBlockIndex(int stringId) => stringId & 0x0F;

    private static Encoding Encoding { get; } = Encoding.Unicode;
}
