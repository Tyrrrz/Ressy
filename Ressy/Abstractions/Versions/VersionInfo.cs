using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Version information associated with a portable executable file.
    /// </summary>
    // https://docs.microsoft.com/en-us/windows/win32/menurc/vs-versioninfo
    // https://docs.microsoft.com/en-us/windows/win32/api/verrsrc/ns-verrsrc-vs_fixedfileinfo
    // https://docs.microsoft.com/en-us/windows/win32/menurc/stringfileinfo
    // https://docs.microsoft.com/en-us/windows/win32/menurc/stringtable
    public record VersionInfo(
        Version FileVersion,
        Version ProductVersion,
        FileFlags FileFlags,
        FileOperatingSystem FileOperatingSystem,
        FileType FileType,
        FileSubType FileSubtype,
        DateTimeOffset FileTimestamp,
        IReadOnlyDictionary<string, string> Attributes,
        IReadOnlyList<TranslationInfo> Translations)
    {
        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public static VersionInfo Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.Unicode);

            // -- VS_VERSIONINFO

            // wLength, wValueLength, wType
            reader.BaseStream.Seek(6, SeekOrigin.Begin);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "VS_VERSION_INFO", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing VS_VERSION_INFO.");

            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // -- VS_FIXEDFILEINFO

            // dwSignature
            if (reader.ReadUInt32() != 0xFEEF04BD)
                throw new InvalidOperationException("Not a valid version resource: missing VS_FIXEDFILEINFO.");

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

            // -- StringFileInfo

            // wLength, wValueLength, wType
            reader.BaseStream.Seek(6, SeekOrigin.Current);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "StringFileInfo", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing StringFileInfo.");

            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // -- StringTable[1..]

            // wLength
            var stringTableEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength, wType
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // szKey
            reader.BaseStream.Seek(16, SeekOrigin.Current);

            // -- String[1..]
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

            // -- VarFileInfo

            // wLength
            var varFileInfoEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength, wType
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "VarFileInfo", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing VarFileInfo.");

            // -- Var

            // Padding
            reader.BaseStream.SeekTo32BitBoundary();

            // wLength, wValueLength, wType
            reader.BaseStream.Seek(6, SeekOrigin.Current);

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "Translation", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing Translation.");

            // Value[1..]
            var translations = new List<TranslationInfo>();
            while (reader.BaseStream.Position < varFileInfoEndPosition)
            {
                // Padding
                reader.BaseStream.SeekTo32BitBoundary();

                var (codepage, languageId) = BitPack.Split(reader.ReadUInt32());
                translations.Add(new TranslationInfo(languageId, codepage));
            }

            return new VersionInfo(
                fileVersion,
                productVersion,
                fileFlags,
                fileOperatingSystem,
                fileType,
                fileSubType,
                fileTimestamp,
                attributes,
                translations
            );
        }
    }
}