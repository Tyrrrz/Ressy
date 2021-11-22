namespace Ressy.Utils
{
    internal static class BitPack
    {
        public static (ushort mostSignificant, ushort leastSignificant) Split(uint value) =>
            ((ushort)(value >> 16), (ushort)(value & 0xFFFF));

        public static uint Merge(ushort mostSignificant, ushort leastSignificant) =>
            (uint)(mostSignificant << 16) | leastSignificant;
    }
}