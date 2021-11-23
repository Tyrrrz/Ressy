using System;
using System.Collections.Generic;
using System.IO;
using Ressy.Utils;

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

                using (stream.CreatePortal(dataOffset).Jump())
                {
                    var data = reader.ReadBytes((int)dataLength);

                    icons[i] = new Icon(
                        width,
                        height,
                        colorCount,
                        colorPlanes,
                        bitsPerPixel,
                        data
                    );
                }
            }

            return new IconGroup(icons);
        }
    }
}