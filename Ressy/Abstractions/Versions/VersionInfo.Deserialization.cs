using System;
using System.IO;
using System.Text;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Versions
{
    public partial class VersionInfo
    {
        private static void ReadFixedFileInfo(BinaryReader reader, VersionInfoBuilder builder)
        {
            // dwSignature
            if (reader.ReadUInt32() != 0xFEEF04BD)
                throw new InvalidOperationException("Not a valid version resource: missing 'VS_FIXEDFILEINFO'.");

            // dwStrucVersion
            _ = reader.ReadUInt32();

            // dwFileVersionMS, dwFileVersionLS
            var (fileVersionMajor, fileVersionMinor) = BitPack.Split(reader.ReadUInt32());
            var (fileVersionBuild, fileVersionRevision) = BitPack.Split(reader.ReadUInt32());

            builder.SetFileVersion(new Version(
                fileVersionMajor,
                fileVersionMinor,
                fileVersionBuild,
                fileVersionRevision
            ), false);

            // dwProductVersionMS, dwProductVersionLS
            var (productVersionMajor, productVersionMinor) = BitPack.Split(reader.ReadUInt32());
            var (productVersionBuild, productVersionRevision) = BitPack.Split(reader.ReadUInt32());

            builder.SetProductVersion(new Version(
                productVersionMajor,
                productVersionMinor,
                productVersionBuild,
                productVersionRevision
            ), false);

            // dwFileFlagsMask
            _ = reader.ReadUInt32();

            // dwFileFlags
            builder.SetFileFlags((FileFlags)reader.ReadUInt32());

            // dwFileOS
            builder.SetFileOperatingSystem((FileOperatingSystem)reader.ReadUInt32());

            // dwFileType
            builder.SetFileType((FileType)reader.ReadUInt32());

            // dwFileSubtype
            builder.SetFileSubType((FileSubType)reader.ReadUInt32());

            // dwFileDateMS, dwFileDateLS (never actually used by Win32)
            _ = reader.ReadUInt64();
        }

        private static void ReadStringFileInfo(BinaryReader reader, VersionInfoBuilder builder)
        {
            // Padding
            reader.SkipPadding();

            // wLength
            var stringTableEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength
            _ = reader.ReadUInt16();

            // wType
            _ = reader.ReadUInt16();

            // szKey (contains language & code page; do we need it?)
            _ = reader.ReadStringNullTerminated();

            // -- String
            while (reader.BaseStream.Position < stringTableEndPosition)
            {
                // Padding
                reader.SkipPadding();

                // wLength
                _ = reader.ReadUInt16();

                // wValueLength
                _ = reader.ReadUInt16();

                // wType
                _ = reader.ReadUInt16();

                // szKey
                var name = reader.ReadStringNullTerminated();

                // Padding
                reader.SkipPadding();

                // Value
                var value = reader.ReadStringNullTerminated();

                builder.SetAttribute(name, value);
            }
        }

        private static void ReadVarFileInfo(BinaryReader reader, VersionInfoBuilder builder)
        {
            // Padding
            reader.SkipPadding();

            // wLength
            var varFileInfoEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength
            _ = reader.ReadUInt16();

            // wType
            _ = reader.ReadUInt16();

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "Translation", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing 'Translation'.");

            // -- Var
            while (reader.BaseStream.Position < varFileInfoEndPosition)
            {
                // Padding
                reader.SkipPadding();

                // Value
                var (codepage, languageId) = BitPack.Split(reader.ReadUInt32());
                builder.AddTranslation(languageId, codepage);
            }
        }

        internal static VersionInfo Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.Unicode);

            var builder = new VersionInfoBuilder();

            // -- VS_VERSIONINFO

            // wLength
            _ = reader.ReadUInt16();

            // wValueLength
            _ = reader.ReadUInt16();

            // wType
            _ = reader.ReadUInt16();

            // szKey
            if (!string.Equals(reader.ReadStringNullTerminated(), "VS_VERSION_INFO", StringComparison.Ordinal))
                throw new InvalidOperationException("Not a valid version resource: missing 'VS_VERSION_INFO'.");

            // Padding
            reader.SkipPadding();

            // -- VS_FIXEDFILEINFO
            ReadFixedFileInfo(reader, builder);

            // Padding
            reader.SkipPadding();

            // Optional StringFileInfo and VarInfo, in any order
            while (!reader.IsEndOfStream())
            {
                // wLength
                _ = reader.ReadUInt16();

                // wValueLength
                _ = reader.ReadUInt16();

                // wType
                _ = reader.ReadUInt16();

                // szKey
                var key = reader.ReadStringNullTerminated();

                // -- StringFileInfo
                if (string.Equals(key, "StringFileInfo", StringComparison.Ordinal))
                {
                    ReadStringFileInfo(reader, builder);
                }
                // -- VarFileInfo
                else if (string.Equals(key, "VarFileInfo", StringComparison.Ordinal))
                {
                    ReadVarFileInfo(reader, builder);
                }
                else
                {
                    throw new InvalidOperationException($"Not a valid version resource: unexpected key '{key}'.");
                }
            }

            return builder.Build();
        }
    }
}