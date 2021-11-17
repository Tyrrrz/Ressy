namespace Ressy.Abstractions.Icons
{
    internal class Icon
    {
        public byte Width { get; }

        public byte Height { get; }

        public byte ColorCount { get; }

        public ushort ColorPlanes { get; }

        public ushort BitsPerPixel { get; }

        public byte[] Data { get; }

        public Icon(byte width, byte height, byte colorCount, ushort colorPlanes, ushort bitsPerPixel, byte[] data)
        {
            Width = width;
            Height = height;
            ColorCount = colorCount;
            ColorPlanes = colorPlanes;
            BitsPerPixel = bitsPerPixel;
            Data = data;
        }
    }
}