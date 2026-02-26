using System.Collections.Generic;
using System.IO;

namespace Ressy.MultilingualUserInterface;

public partial class MuiInfo
{
    // Header is always 0x7C (124) bytes
    private const uint HeaderSize = 0x7Cu;

    private static (uint offset, uint size, byte[] bytes) BuildLanguageEntry(
        string? value,
        ref uint currentOffset
    )
    {
        if (value is null)
            return (0, 0, []);

        var bytes = Encoding.GetBytes(value + '\0');
        var offset = currentOffset;
        currentOffset += (uint)bytes.Length;
        return (offset, (uint)bytes.Length, bytes);
    }

    private static (uint offset, uint size, byte[] bytes) BuildTypeIDListEntry(
        IReadOnlyList<ResourceType> list,
        ref uint currentOffset
    )
    {
        if (list.Count == 0)
            return (0, 0, []);

        var bytes = new byte[(list.Count + 1) * 2]; // items + null terminator
        for (var i = 0; i < list.Count; i++)
        {
            var code = (ushort)(list[i].Code ?? 0);
            bytes[i * 2] = (byte)(code & 0xFF);
            bytes[i * 2 + 1] = (byte)(code >> 8);
        }
        // null terminator is already 0 (default byte value)

        var offset = currentOffset;
        currentOffset += (uint)bytes.Length;
        return (offset, (uint)bytes.Length, bytes);
    }

    internal byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Compute offsets for variable-length data (starts right after the header)
        var currentOffset = HeaderSize;

        var (typeIDMainOffset, typeIDMainSize, typeIDMainBytes) = BuildTypeIDListEntry(
            MainResourceTypes,
            ref currentOffset
        );

        var (typeIDMuiOffset, typeIDMuiSize, typeIDMuiBytes) = BuildTypeIDListEntry(
            FallbackResourceTypes,
            ref currentOffset
        );

        var (languageOffset, languageSize, languageBytes) = BuildLanguageEntry(
            Language,
            ref currentOffset
        );

        var (fallbackOffset, fallbackSize, fallbackBytes) = BuildLanguageEntry(
            FallbackLanguage,
            ref currentOffset
        );

        var (ultimateFallbackOffset, ultimateFallbackSize, ultimateFallbackBytes) =
            BuildLanguageEntry(UltimateFallbackLanguage, ref currentOffset);

        // Write header (124 bytes)

        // dwSignature
        writer.Write(MuiSignature);

        // dwSize (total binary size including all variable-length data appended after the header)
        writer.Write(currentOffset);

        // dwVersion
        writer.Write(MuiVersion);

        // dwPathType (reserved, always 0)
        writer.Write(0u);

        // dwFileType
        writer.Write((uint)FileType);

        // dwSystemAttributes
        writer.Write(0u);

        // dwUltimateFallbackLocation
        writer.Write(0u);

        // abChecksum[16]
        writer.Write(Checksum.Length == 16 ? Checksum : new byte[16]);

        // abServiceChecksum[16]
        writer.Write(ServiceChecksum.Length == 16 ? ServiceChecksum : new byte[16]);

        // dwTypeNameMainOffset, dwTypeNameMainSize (named resource types in main file)
        writer.Write(0u);
        writer.Write(0u);

        // dwTypeIDMainOffset, dwTypeIDMainSize (ordinal resource type IDs in main file)
        writer.Write(typeIDMainOffset);
        writer.Write(typeIDMainSize);

        // dwTypeNameMuiOffset, dwTypeNameMuiSize (named resource types in MUI satellite)
        writer.Write(0u);
        writer.Write(0u);

        // dwTypeIDMuiOffset, dwTypeIDMuiSize (ordinal resource type IDs in MUI satellite)
        writer.Write(typeIDMuiOffset);
        writer.Write(typeIDMuiSize);

        // dwLanguageOffset, dwLanguageSize
        writer.Write(languageOffset);
        writer.Write(languageSize);

        // dwFallbackLanguageOffset, dwFallbackLanguageSize
        writer.Write(fallbackOffset);
        writer.Write(fallbackSize);

        // dwUltimateFallbackLanguageOffset, dwUltimateFallbackLanguageSize
        writer.Write(ultimateFallbackOffset);
        writer.Write(ultimateFallbackSize);

        // Reserved padding to complete the 124-byte header
        writer.Write(new byte[8]);

        // Write variable-length data
        if (typeIDMainBytes.Length > 0)
            writer.Write(typeIDMainBytes);

        if (typeIDMuiBytes.Length > 0)
            writer.Write(typeIDMuiBytes);

        if (languageBytes.Length > 0)
            writer.Write(languageBytes);

        if (fallbackBytes.Length > 0)
            writer.Write(fallbackBytes);

        if (ultimateFallbackBytes.Length > 0)
            writer.Write(ultimateFallbackBytes);

        writer.Flush();

        return stream.ToArray();
    }
}
