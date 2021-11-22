using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Versions
{
    public partial class VersionInfo
    {
        private static IReadOnlyDictionary<string, string> DeserializeAttributes(BinaryReader reader)
        {
            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // wLength
            var stringTableEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength, wType
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // szKey (contains language & code page; do we need it?)
            _ = reader.ReadStringNullTerminated();

            // Children
            var attributes = new Dictionary<string, string>(StringComparer.Ordinal);
            while (reader.BaseStream.Position < stringTableEndPosition)
            {
                // Padding
                reader.BaseStream.SeekTo32BitBoundary();

                // wLength, wValueLength, wType
                reader.BaseStream.Seek(6, SeekOrigin.Current);

                // szKey
                var attributeName = reader.ReadStringNullTerminated();

                // Padding
                reader.BaseStream.SeekTo32BitBoundary();

                // Value
                var attributeValue = reader.ReadStringNullTerminated();

                attributes[attributeName] = attributeValue;
            }

            return attributes;
        }

        private static IReadOnlyList<TranslationInfo> DeserializeTranslations(BinaryReader reader)
        {
            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // wLength
            var varFileInfoEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength, wType
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "Translation", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing 'Translation'.");

            // Children
            var translations = new List<TranslationInfo>();
            while (reader.BaseStream.Position < varFileInfoEndPosition)
            {
                // Padding
                reader.BaseStream.SeekTo32BitBoundary();

                var (codepage, languageId) = BitPack.Split(reader.ReadUInt32());
                translations.Add(new TranslationInfo(languageId, codepage));
            }

            return translations;
        }

        internal static VersionInfo Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.Unicode);

            // -- VS_VERSIONINFO

            // wLength, wValueLength, wType
            reader.BaseStream.Seek(6, SeekOrigin.Begin);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "VS_VERSION_INFO", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing 'VS_VERSION_INFO'.");

            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // -- VS_FIXEDFILEINFO

            // dwSignature
            if (reader.ReadUInt32() != 0xFEEF04BD)
                throw new InvalidOperationException("Not a valid version resource: missing 'VS_FIXEDFILEINFO'.");

            // dwStrucVersion
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // dwFileVersionMS, dwFileVersionLS
            var fileVersion = reader.ReadVersion();

            // dwProductVersionMS, dwProductVersionLS
            var productVersion = reader.ReadVersion();

            // dwFileFlagsMask
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // dwFileFlags
            var fileFlags = (FileFlags)reader.ReadUInt32();

            // dwFileOS
            var fileOperatingSystem = (FileOperatingSystem)reader.ReadUInt32();

            // dwFileType
            var fileType = (FileType)reader.ReadUInt32();

            // dwFileSubtype
            var fileSubType = (FileSubType)reader.ReadUInt32();

            // dwFileDateMS, dwFileDateLS
            var fileTimestamp =
                new DateTimeOffset(1601, 01, 01, 00, 00, 00, TimeSpan.Zero) +
                TimeSpan.FromSeconds(reader.ReadUInt64() / 10e7);

            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // Optional StringFileInfo and VarInfo, in any order
            var attributes = default(IReadOnlyDictionary<string, string>);
            var translations = default(IReadOnlyList<TranslationInfo>);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // wLength, wValueLength, wType
                reader.BaseStream.Seek(6, SeekOrigin.Current);

                // szKey
                var key = reader.ReadStringNullTerminated();

                // -- StringFileInfo
                if (string.Equals(key, "StringFileInfo", StringComparison.Ordinal))
                {
                    attributes = DeserializeAttributes(reader);
                }
                // -- VarFileInfo
                else if (string.Equals(key, "VarFileInfo", StringComparison.Ordinal))
                {
                    translations = DeserializeTranslations(reader);
                }
                else
                {
                    throw new InvalidOperationException($"Not a valid version resource: unexpected key '{key}'.");
                }
            }

            return new VersionInfo(
                fileVersion,
                productVersion,
                fileFlags,
                fileOperatingSystem,
                fileType,
                fileSubType,
                fileTimestamp,
                attributes ?? new Dictionary<string, string>(),
                translations ?? Array.Empty<TranslationInfo>()
            );
        }
    }
}