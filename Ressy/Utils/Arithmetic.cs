namespace Ressy.Utils;

internal static class Arithmetic
{
    public static int AlignUp(int value, int alignment) =>
        alignment > 0 ? (int)(((long)value + alignment - 1) / alignment * alignment) : value;

    public static uint AlignUp(uint value, uint alignment) =>
        alignment > 0 ? (uint)(((ulong)value + alignment - 1UL) / alignment * alignment) : value;
}
