using System;
using System.Collections.Generic;
using System.IO;

namespace Ressy.Abstractions.Icons
{
    // https://en.wikipedia.org/wiki/ICO_(file_format)#Outline
    internal partial class IconFile : IDisposable
    {
        private readonly BinaryReader _reader;

        public IconFile(BinaryReader reader) => _reader = reader;

        public byte[] GetHeaderData()
        {
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            return _reader.ReadBytes(6);
        }

        public IEnumerable<IconFileEntry> GetIcons()
        {
            _reader.BaseStream.Seek(4, SeekOrigin.Begin);
            var iconCount = _reader.ReadUInt16();

            for (var i = 0; i < iconCount; i++)
            {
                var metaDataOffset = 6 + i * 16;

                // Seek to length and offset
                _reader.BaseStream.Seek(metaDataOffset + 8, SeekOrigin.Begin);

                var iconDataLength = _reader.ReadUInt32();
                var iconDataOffset = _reader.ReadUInt32();

                yield return new IconFileEntry(_reader, metaDataOffset, (int) iconDataOffset, (int) iconDataLength);
            }
        }

        public void Dispose() => _reader.Dispose();
    }

    internal partial class IconFile
    {
        public static IconFile Open(Stream stream)
        {
            var reader = new BinaryReader(stream);

            // Verify file format
            if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1)
                throw new InvalidOperationException("Specified file is not a valid ICO file.");

            return new IconFile(reader);
        }
    }
}