using System;
using System.Collections.Generic;
using System.IO;

namespace Ressy.MultilingualUserInterface;

public partial class MuiInfo
{
    // https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
    private const uint MuiSignature = 0xFECDFECDu;
    private const uint MuiVersion = 0x00010001u;

    private static string? ReadLanguageString(byte[] data, uint offset, uint size)
    {
        if (offset == 0 || size == 0)
            return null;

        // Strings are null-terminated UTF-16 LE; size includes the null terminator (2 bytes)
        var byteLength = (int)size - 2;
        if (byteLength <= 0)
            return null;

        return Encoding.GetString(data, (int)offset, byteLength);
    }

    private static IReadOnlyList<ResourceType> ReadTypeIDList(byte[] data, uint offset, uint size)
    {
        if (offset == 0 || size == 0)
            return [];

        var list = new List<ResourceType>();
        var pos = (int)offset;
        var end = pos + (int)size;

        while (pos + 1 < end && pos + 1 < data.Length)
        {
            var id = (int)BitConverter.ToUInt16(data, pos);
            pos += 2;
            if (id == 0)
                break;
            list.Add(ResourceType.FromCode(id));
        }

        return list;
    }

    internal static MuiInfo Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // dwSignature
        var signature = reader.ReadUInt32();
        if (signature != MuiSignature)
            throw new InvalidOperationException("Invalid MUI resource: wrong signature.");

        // dwSize (total binary size)
        _ = reader.ReadUInt32();

        // dwVersion (currently only 0x00010001 is known; not validated to allow forward compatibility)
        _ = reader.ReadUInt32();

        // dwPathType (reserved)
        _ = reader.ReadUInt32();

        // dwFileType
        var fileType = (MuiFileType)reader.ReadUInt32();

        // dwSystemAttributes
        _ = reader.ReadUInt32();

        // dwUltimateFallbackLocation
        _ = reader.ReadUInt32();

        // abChecksum[16]
        var checksum = reader.ReadBytes(16);

        // abServiceChecksum[16]
        var serviceChecksum = reader.ReadBytes(16);

        // dwTypeNameMainOffset, dwTypeNameMainSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwTypeIDMainOffset, dwTypeIDMainSize
        var typeIDMainOffset = reader.ReadUInt32();
        var typeIDMainSize = reader.ReadUInt32();

        // dwTypeNameMuiOffset, dwTypeNameMuiSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwTypeIDMuiOffset, dwTypeIDMuiSize
        var typeIDMuiOffset = reader.ReadUInt32();
        var typeIDMuiSize = reader.ReadUInt32();

        // dwLanguageOffset, dwLanguageSize
        var languageOffset = reader.ReadUInt32();
        var languageSize = reader.ReadUInt32();

        // dwFallbackLanguageOffset, dwFallbackLanguageSize
        var fallbackOffset = reader.ReadUInt32();
        var fallbackSize = reader.ReadUInt32();

        // dwUltimateFallbackLanguageOffset, dwUltimateFallbackLanguageSize
        var ultimateFallbackOffset = reader.ReadUInt32();
        var ultimateFallbackSize = reader.ReadUInt32();

        var mainResourceTypes = ReadTypeIDList(data, typeIDMainOffset, typeIDMainSize);
        var fallbackResourceTypes = ReadTypeIDList(data, typeIDMuiOffset, typeIDMuiSize);
        var language = ReadLanguageString(data, languageOffset, languageSize);
        var fallbackLanguage = ReadLanguageString(data, fallbackOffset, fallbackSize);
        var ultimateFallbackLanguage = ReadLanguageString(
            data,
            ultimateFallbackOffset,
            ultimateFallbackSize
        );

        return new MuiInfo(
            fileType,
            checksum,
            serviceChecksum,
            mainResourceTypes,
            fallbackResourceTypes,
            language,
            fallbackLanguage,
            ultimateFallbackLanguage
        );
    }
}
