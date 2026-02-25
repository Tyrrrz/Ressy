using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
public sealed partial class PortableExecutable : IDisposable
{
    // PE format constants
    private const ushort DosMagic = 0x5A4D; // 'MZ'
    private const uint PeSignature = 0x4550; // 'PE\0\0'
    private const ushort Pe32Magic = 0x10B;
    private const ushort Pe32PlusMagic = 0x20B;
    private const uint NameStringFlag = 0x80000000u;
    private const uint SubdirectoryFlag = 0x80000000u;
    private const int SectionHeaderSize = 40;

    // IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE
    private const uint RsrcSectionCharacteristics = 0xC0000040u;

    private readonly Stream _stream;
    private readonly bool _disposeStream;
    private PeInfo _info;

    /// <summary>
    /// Path to the portable executable image file, or <c>null</c> if opened from a stream.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Opens the portable executable at the specified file path.
    /// </summary>
    public PortableExecutable(string filePath)
    {
        FilePath = filePath;
        _stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        _disposeStream = true;
        _info = ParsePeInfo(_stream);
    }

    /// <summary>
    /// Wraps a seekable, readable, and writable stream as a portable executable.
    /// </summary>
    /// <param name="stream">A seekable stream positioned at the start of a PE file.</param>
    /// <param name="disposeStream">Whether to dispose the stream when this instance is disposed.</param>
    public PortableExecutable(Stream stream, bool disposeStream = false)
    {
        _stream = stream;
        _disposeStream = disposeStream;
        _info = ParsePeInfo(stream);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposeStream)
            _stream.Dispose();
    }

    private sealed class PeInfo
    {
        public required bool IsPe32Plus { get; init; }
        public required uint SectionAlignment { get; init; }
        public required uint FileAlignment { get; init; }
        public required int DataDir2FileOffset { get; init; }
        public required int SizeOfImageFileOffset { get; init; }
        public required int NumberOfSectionsFileOffset { get; init; }
        public required int FirstSectionHeaderFileOffset { get; init; }
        public required int SizeOfHeadersValue { get; init; }
        public List<SectionInfo> Sections { get; } = new();
        public int RsrcSectionIndex { get; set; } = -1;
    }

    private sealed class SectionInfo
    {
        public required string Name { get; init; }
        public required uint VirtualSize { get; init; }
        public required uint VirtualAddress { get; init; }
        public required uint SizeOfRawData { get; init; }
        public required uint PointerToRawData { get; init; }
        public required uint Characteristics { get; init; }
        public required int HeaderFileOffset { get; init; }
    }

    private static PeInfo ParsePeInfo(Stream stream)
    {
        if (stream.Length < 64)
            throw new InvalidDataException("File is too small to be a valid PE file.");

        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        if (reader.ReadUInt16() != DosMagic)
        {
            throw new InvalidDataException(
                "File does not have a valid PE signature (MZ header not found)."
            );
        }

        stream.Position = 60;
        var peOffset = (int)reader.ReadUInt32();

        if (peOffset + 4 > stream.Length)
        {
            throw new InvalidDataException(
                "File does not have a valid PE signature (PE header not found)."
            );
        }

        stream.Position = peOffset;
        if (reader.ReadUInt32() != PeSignature)
        {
            throw new InvalidDataException(
                "File does not have a valid PE signature (PE header not found)."
            );
        }

        var coffHeaderOffset = peOffset + 4;
        if (coffHeaderOffset + 20 > stream.Length)
            throw new InvalidDataException("PE COFF header is truncated.");

        stream.Position = coffHeaderOffset + 2;
        var numberOfSections = (int)reader.ReadUInt16();

        stream.Position = coffHeaderOffset + 16;
        var sizeOfOptionalHeader = (int)reader.ReadUInt16();

        var optHeaderOffset = coffHeaderOffset + 20;
        if (optHeaderOffset + 2 > stream.Length)
            throw new InvalidDataException("PE optional header is missing.");

        stream.Position = optHeaderOffset;
        var optMagic = reader.ReadUInt16();
        var isPe32Plus = optMagic switch
        {
            Pe32Magic => false,
            Pe32PlusMagic => true,
            _ => throw new InvalidDataException(
                $"Unknown PE optional header magic: 0x{optMagic:X4}."
            ),
        };

        // These fields are at the same offsets in both PE32 and PE32+
        // Minimum required size: PE32 needs 64 bytes (0..63), PE32+ needs 80 bytes (0..79)
        var minOptHeaderSize = isPe32Plus ? 80 : 64;
        if (optHeaderOffset + minOptHeaderSize > stream.Length)
            throw new InvalidDataException("PE optional header is truncated.");

        stream.Position = optHeaderOffset + 32;
        var sectionAlignment = reader.ReadUInt32();
        var fileAlignment = reader.ReadUInt32(); // optHeaderOffset + 36

        var sizeOfImageFileOffset = optHeaderOffset + 56;

        stream.Position = optHeaderOffset + 60;
        var sizeOfHeadersValue = (int)reader.ReadUInt32();

        // DataDirectory base: PE32 = optHeader + 96, PE32+ = optHeader + 112
        // DataDirectory[2] (Resource) is at base + 2 * 8 = base + 16
        var dataDirBase = optHeaderOffset + (isPe32Plus ? 112 : 96);
        if (dataDirBase + 24 > stream.Length)
        {
            throw new InvalidDataException(
                "PE optional header is too small to contain the resource data directory entry."
            );
        }

        var dataDir2FileOffset = dataDirBase + 16;

        // Section headers begin right after the optional header
        var firstSectionHeaderOffset = optHeaderOffset + sizeOfOptionalHeader;

        var info = new PeInfo
        {
            IsPe32Plus = isPe32Plus,
            SectionAlignment = sectionAlignment,
            FileAlignment = fileAlignment,
            DataDir2FileOffset = dataDir2FileOffset,
            SizeOfImageFileOffset = sizeOfImageFileOffset,
            NumberOfSectionsFileOffset = coffHeaderOffset + 2,
            FirstSectionHeaderFileOffset = firstSectionHeaderOffset,
            SizeOfHeadersValue = sizeOfHeadersValue,
        };

        for (var i = 0; i < numberOfSections; i++)
        {
            var headerOffset = firstSectionHeaderOffset + i * SectionHeaderSize;
            if (headerOffset + SectionHeaderSize > stream.Length)
                break;

            stream.Position = headerOffset;
            var nameBytes = reader.ReadBytes(8);
            var sectionName = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

            stream.Position = headerOffset + 8;
            var virtualSize = reader.ReadUInt32();
            var virtualAddress = reader.ReadUInt32();
            var sizeOfRawData = reader.ReadUInt32();
            var pointerToRawData = reader.ReadUInt32();

            stream.Position = headerOffset + 36;
            var characteristics = reader.ReadUInt32();

            var section = new SectionInfo
            {
                Name = sectionName,
                VirtualSize = virtualSize,
                VirtualAddress = virtualAddress,
                SizeOfRawData = sizeOfRawData,
                PointerToRawData = pointerToRawData,
                Characteristics = characteristics,
                HeaderFileOffset = headerOffset,
            };

            info.Sections.Add(section);
            if (sectionName == ".rsrc")
                info.RsrcSectionIndex = i;
        }

        return info;
    }
}
