using System;
using System.IO;
using System.Text;
using PowerKit;
using PowerKit.Extensions;

namespace Ressy.Icons;

internal partial class IconGroup
{
    private static IconGroup DeserializeFromSeekable(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1)
        {
            throw new InvalidOperationException(
                "Invalid ICO format: missing or unexpected magic number."
            );
        }

        var iconCount = reader.ReadUInt16();
        var icons = new Icon[iconCount];

        // Icon directory
        for (var i = 0; i < iconCount; i++)
        {
            var width = reader.ReadByte();
            var height = reader.ReadByte();
            var colorCount = reader.ReadByte();
            _ = reader.ReadByte(); // reserved
            var colorPlanes = reader.ReadUInt16();
            var bitsPerPixel = reader.ReadUInt16();
            var dataLength = reader.ReadUInt32();
            var dataOffset = reader.ReadUInt32();

            var data = new byte[dataLength];
            using (reader.BaseStream.CreatePortal(dataOffset).Jump())
                reader.BaseStream.ReadExactly(data);

            icons[i] = new Icon(width, height, colorCount, colorPlanes, bitsPerPixel, data);
        }

        return new IconGroup(icons);
    }

    public static IconGroup Deserialize(Stream stream)
    {
        if (!stream.CanSeek)
        {
            using var seekableStream = new MemoryReadStream(stream);
            return DeserializeFromSeekable(seekableStream);
        }

        return DeserializeFromSeekable(stream);
    }
}
