using System;
using System.IO;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.HighLevel.Versions;

public partial class VersionInfo
{
    private static void ReadFixedFileInfo(BinaryReader reader, VersionInfoBuilder builder)
    {
        // dwSignature
        if (reader.ReadUInt32() != 0xFEEF04BD)
            throw new InvalidOperationException(
                "Invalid version resource: missing 'VS_FIXEDFILEINFO'."
            );

        // dwStrucVersion
        _ = reader.ReadUInt32();

        // dwFileVersionMS, dwFileVersionLS
        var (fileVersionMajor, fileVersionMinor) = BitPack.Split(reader.ReadUInt32());
        var (fileVersionBuild, fileVersionRevision) = BitPack.Split(reader.ReadUInt32());

        builder.SetFileVersion(
            new Version(fileVersionMajor, fileVersionMinor, fileVersionBuild, fileVersionRevision),
            false
        );

        // dwProductVersionMS, dwProductVersionLS
        var (productVersionMajor, productVersionMinor) = BitPack.Split(reader.ReadUInt32());
        var (productVersionBuild, productVersionRevision) = BitPack.Split(reader.ReadUInt32());

        builder.SetProductVersion(
            new Version(
                productVersionMajor,
                productVersionMinor,
                productVersionBuild,
                productVersionRevision
            ),
            false
        );

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

    private static void ReadStringTable(BinaryReader reader, VersionInfoBuilder builder)
    {
        // wLength
        var stringTableEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

        // wValueLength
        _ = reader.ReadUInt16();

        // wType
        _ = reader.ReadUInt16();

        // szKey
        var (languageId, codePageId) = BitPack.Split(
            Convert.ToUInt32(reader.ReadNullTerminatedString(), 16)
        );

        var language = new Language(languageId);
        var codePage = new CodePage(codePageId);

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
            var name = reader.ReadNullTerminatedString();

            // Padding
            reader.SkipPadding();

            // Value
            var value = reader.ReadNullTerminatedString();

            builder.SetAttribute(name, value, language, codePage);

            // There is some padding between the strings, but it doesn't seem to be defined in the spec
            // and every resource compiler appears to choose it arbitrarily.
            // So in order to work around it, we'll just skip all zero bytes we encounter.
            reader.SkipZeroes(stringTableEndPosition - reader.BaseStream.Position);
        }
    }

    internal static VersionInfo Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding);

        var builder = new VersionInfoBuilder();

        // -- VS_VERSIONINFO

        // wLength
        _ = reader.ReadUInt16();

        // wValueLength
        _ = reader.ReadUInt16();

        // wType
        _ = reader.ReadUInt16();

        // szKey
        if (
            !string.Equals(
                reader.ReadNullTerminatedString(),
                "VS_VERSION_INFO",
                StringComparison.Ordinal
            )
        )
            throw new InvalidOperationException(
                "Invalid version resource: missing 'VS_VERSION_INFO'."
            );

        // Padding
        reader.SkipPadding();

        // -- VS_FIXEDFILEINFO
        ReadFixedFileInfo(reader, builder);

        // Optional StringFileInfo and VarInfo, in any order
        while (!reader.IsEndOfStream())
        {
            // Padding
            reader.SkipPadding();

            // wLength
            var childEndPosition = reader.BaseStream.Position + reader.ReadUInt16();

            // wValueLength
            _ = reader.ReadUInt16();

            // wType
            _ = reader.ReadUInt16();

            // szKey
            var key = reader.ReadNullTerminatedString();

            // -- StringFileInfo
            if (string.Equals(key, "StringFileInfo", StringComparison.Ordinal))
            {
                // Can contain 1 or more string tables
                while (reader.BaseStream.Position < childEndPosition)
                {
                    // Padding
                    reader.SkipPadding();

                    ReadStringTable(reader, builder);
                }
            }
            // -- VarFileInfo
            else if (string.Equals(key, "VarFileInfo", StringComparison.Ordinal))
            {
                // Padding
                reader.SkipPadding();

                // There is nothing useful here, since all of the required information can be extracted
                // from StringFileInfo anyway. So we can just skip it.
                reader.BaseStream.Seek(childEndPosition, SeekOrigin.Begin);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid version resource: unexpected key '{key}'."
                );
            }
        }

        return builder.Build();
    }
}
