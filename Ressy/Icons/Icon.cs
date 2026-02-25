namespace Ressy.Icons;

internal class Icon(
    byte width,
    byte height,
    byte colorCount,
    ushort colorPlanes,
    ushort bitsPerPixel,
    byte[] data
)
{
    public byte Width { get; } = width;

    public byte Height { get; } = height;

    public byte ColorCount { get; } = colorCount;

    public ushort ColorPlanes { get; } = colorPlanes;

    public ushort BitsPerPixel { get; } = bitsPerPixel;

    public byte[] Data { get; } = data;
}
