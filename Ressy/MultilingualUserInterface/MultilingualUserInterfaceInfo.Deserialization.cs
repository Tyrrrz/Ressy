using System;
using System.IO;

namespace Ressy.MultilingualUserInterface;

public partial class MultilingualUserInterfaceInfo
{
    // https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
    private const uint MuiSignature = 0xFECDFECDu;

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

    internal static MultilingualUserInterfaceInfo Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // dwSignature
        var signature = reader.ReadUInt32();
        if (signature != MuiSignature)
            throw new InvalidOperationException("Invalid MUI resource: wrong signature.");

        // dwHeaderSize
        _ = reader.ReadUInt32();

        // dwFileType
        var fileType = (MuiFileType)reader.ReadUInt32();

        // dwSystemAttributes
        _ = reader.ReadUInt32();

        // dwUltimateFallbackLocation
        _ = reader.ReadUInt32();

        // bReserved[8]
        _ = reader.ReadBytes(8);

        // abChecksum[16]
        _ = reader.ReadBytes(16);

        // abServiceChecksum[16]
        _ = reader.ReadBytes(16);

        // dwTypeNameListOffset, dwTypeNameListSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwTypeIDFallbackListOffset, dwTypeIDFallbackListSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwTypeIDMainListOffset, dwTypeIDMainListSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwNameListOffset, dwNameListSize
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();

        // dwLanguageOffset, dwLanguageSize
        var languageOffset = reader.ReadUInt32();
        var languageSize = reader.ReadUInt32();

        // dwFallbackLanguageOffset, dwFallbackLanguageSize
        var fallbackOffset = reader.ReadUInt32();
        var fallbackSize = reader.ReadUInt32();

        // dwUltimateFallbackLanguageOffset, dwUltimateFallbackLanguageSize
        var ultimateFallbackOffset = reader.ReadUInt32();
        var ultimateFallbackSize = reader.ReadUInt32();

        var language = ReadLanguageString(data, languageOffset, languageSize);
        var fallbackLanguage = ReadLanguageString(data, fallbackOffset, fallbackSize);
        var ultimateFallbackLanguage = ReadLanguageString(
            data,
            ultimateFallbackOffset,
            ultimateFallbackSize
        );

        return new MultilingualUserInterfaceInfo(
            fileType,
            language,
            fallbackLanguage,
            ultimateFallbackLanguage
        );
    }
}
