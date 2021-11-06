using System.IO;

namespace Ressy.Utils
{
    internal class IconFileEntry
    {
        private readonly BinaryReader _reader;
        private readonly int _metaDataOffset;
        private readonly int _iconDataOffset;
        private readonly int _iconDataLength;

        public IconFileEntry(BinaryReader reader, int metaDataOffset, int iconDataOffset, int iconDataLength)
        {
            _reader = reader;
            _metaDataOffset = metaDataOffset;
            _iconDataOffset = iconDataOffset;
            _iconDataLength = iconDataLength;
        }

        public byte[] GetMetaData()
        {
            _reader.BaseStream.Seek(_metaDataOffset, SeekOrigin.Begin);
            return _reader.ReadBytes(16);
        }

        public byte[] GetIconData()
        {
            _reader.BaseStream.Seek(_iconDataOffset, SeekOrigin.Begin);
            return _reader.ReadBytes(_iconDataLength);
        }
    }
}