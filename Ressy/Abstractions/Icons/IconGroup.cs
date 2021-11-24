using System;
using System.Collections.Generic;
using System.IO;

namespace Ressy.Abstractions.Icons
{
    // https://en.wikipedia.org/wiki/ICO_(file_format)#Outline
    internal partial class IconGroup
    {
        public IReadOnlyList<Icon> Icons { get; }

        public IconGroup(IReadOnlyList<Icon> icons) => Icons = icons;
    }

    internal partial class IconGroup
    {
        public static IconGroup Deserialize(Stream stream)
        {
            using var reader = new BinaryReader(stream);

            if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1)
                throw new InvalidOperationException("Invalid ICO file format (missing or unexpected magic number).");

            var iconCount = reader.ReadUInt16();
            var icons = new Icon[iconCount];
            var iconDataOffsets = new uint[iconCount];
            var iconDataSets = new byte[iconCount][];

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

                // Will fill this out at a later stage, just need a reference for now
                var data = iconDataSets[i] = new byte[dataLength];
                iconDataOffsets[i] = dataOffset;

                icons[i] = new Icon(
                    width,
                    height,
                    colorCount,
                    colorPlanes,
                    bitsPerPixel,
                    data
                );
            }

            // Icon data
            for (var i = 0; i < iconCount; i++)
            {
                reader.BaseStream.Seek(iconDataOffsets[i], SeekOrigin.Begin);
                reader.Read(iconDataSets[i], 0, iconDataSets[i].Length);
            }

            return new IconGroup(icons);
        }
    }
}