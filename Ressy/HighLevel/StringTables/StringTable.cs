using System.Text;

namespace Ressy.HighLevel.StringTables;

internal partial class StringTable
{
    internal const int BlockSize = 16;

    internal static int GetBlockId(int stringId) => (stringId >> 4) + 1;

    internal static int GetBlockIndex(int stringId) => stringId & 0x0F;

    private static Encoding Encoding { get; } = Encoding.Unicode;
}
