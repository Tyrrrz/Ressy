using System.IO;

namespace Ressy.MultilingualUserInterface;

public partial class MultilingualUserInterfaceInfo
{
    // Header is always 0x7C (124) bytes
    private const uint HeaderSize = 0x7Cu;

    internal byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Compute offsets for variable-length data (starts right after the header)
        var currentOffset = HeaderSize;

        uint languageOffset,
            languageSize;
        byte[]? languageBytes = null;

        if (Language is not null)
        {
            languageOffset = currentOffset;
            languageBytes = Encoding.GetBytes(Language + '\0');
            languageSize = (uint)languageBytes.Length;
            currentOffset += languageSize;
        }
        else
        {
            languageOffset = 0;
            languageSize = 0;
        }

        uint fallbackOffset,
            fallbackSize;
        byte[]? fallbackBytes = null;

        if (FallbackLanguage is not null)
        {
            fallbackOffset = currentOffset;
            fallbackBytes = Encoding.GetBytes(FallbackLanguage + '\0');
            fallbackSize = (uint)fallbackBytes.Length;
            currentOffset += fallbackSize;
        }
        else
        {
            fallbackOffset = 0;
            fallbackSize = 0;
        }

        uint ultimateFallbackOffset,
            ultimateFallbackSize;
        byte[]? ultimateFallbackBytes = null;

        if (UltimateFallbackLanguage is not null)
        {
            ultimateFallbackOffset = currentOffset;
            ultimateFallbackBytes = Encoding.GetBytes(UltimateFallbackLanguage + '\0');
            ultimateFallbackSize = (uint)ultimateFallbackBytes.Length;
        }
        else
        {
            ultimateFallbackOffset = 0;
            ultimateFallbackSize = 0;
        }

        // Write header (124 bytes)

        // dwSignature
        writer.Write(MuiSignature);

        // dwHeaderSize
        writer.Write(HeaderSize);

        // dwFileType
        writer.Write((uint)FileType);

        // dwSystemAttributes
        writer.Write(0u);

        // dwUltimateFallbackLocation
        writer.Write(0u);

        // bReserved[8]
        writer.Write(new byte[8]);

        // abChecksum[16]
        writer.Write(new byte[16]);

        // abServiceChecksum[16]
        writer.Write(new byte[16]);

        // dwTypeNameListOffset, dwTypeNameListSize
        writer.Write(0u);
        writer.Write(0u);

        // dwTypeIDFallbackListOffset, dwTypeIDFallbackListSize
        writer.Write(0u);
        writer.Write(0u);

        // dwTypeIDMainListOffset, dwTypeIDMainListSize
        writer.Write(0u);
        writer.Write(0u);

        // dwNameListOffset, dwNameListSize
        writer.Write(0u);
        writer.Write(0u);

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

        // Write variable-length language strings
        if (languageBytes is not null)
            writer.Write(languageBytes);

        if (fallbackBytes is not null)
            writer.Write(fallbackBytes);

        if (ultimateFallbackBytes is not null)
            writer.Write(ultimateFallbackBytes);

        writer.Flush();

        return stream.ToArray();
    }
}
