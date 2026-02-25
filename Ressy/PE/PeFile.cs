using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy.PE;

// Provides cross-platform reading and writing of native resources in PE (Portable Executable) files.
internal static partial class PeFile
{
    #region Binary helpers

    private static ushort ReadUInt16(byte[] data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));

    private static uint ReadUInt32(byte[] data, int offset) =>
        (uint)(
            data[offset]
            | (data[offset + 1] << 8)
            | (data[offset + 2] << 16)
            | (data[offset + 3] << 24)
        );

    private static void WriteUInt16(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    private static int AlignUp(int value, int alignment) =>
        alignment > 0 ? (int)(((long)value + alignment - 1) / alignment * alignment) : value;

    private static uint AlignUp(uint value, uint alignment) =>
        alignment > 0 ? (uint)(((ulong)value + alignment - 1UL) / alignment * alignment) : value;

    #endregion

    #region PE constants

    private const ushort DosMagic = 0x5A4D; // 'MZ'
    private const uint PeSignature = 0x4550; // 'PE\0\0'
    private const ushort Pe32Magic = 0x10B;
    private const ushort Pe32PlusMagic = 0x20B;
    private const uint NameStringFlag = 0x80000000u;
    private const uint SubdirectoryFlag = 0x80000000u;
    private const int SectionHeaderSize = 40;

    // IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE
    private const uint RsrcSectionCharacteristics = 0xC0000040u;

    #endregion

    #region PE structure types

    private sealed class PeInfo
    {
        public bool IsPe32Plus { get; init; }
        public uint SectionAlignment { get; init; }
        public uint FileAlignment { get; init; }
        public int DataDir2FileOffset { get; init; }
        public int SizeOfImageFileOffset { get; init; }
        public int NumberOfSectionsFileOffset { get; init; }
        public int FirstSectionHeaderFileOffset { get; init; }
        public int SizeOfHeadersValue { get; init; }
        public List<SectionInfo> Sections { get; } = new();
        public int RsrcSectionIndex { get; set; } = -1;
    }

    private sealed class SectionInfo
    {
        public string Name { get; set; } = "";
        public uint VirtualSize { get; set; }
        public uint VirtualAddress { get; set; }
        public uint SizeOfRawData { get; set; }
        public uint PointerToRawData { get; set; }
        public uint Characteristics { get; set; }
        public int HeaderFileOffset { get; set; }
    }

    #endregion

    #region PE parsing

    private static PeInfo ParsePeInfo(byte[] fileBytes)
    {
        if (fileBytes.Length < 64)
            throw new InvalidDataException("File is too small to be a valid PE file.");

        if (ReadUInt16(fileBytes, 0) != DosMagic)
            throw new InvalidDataException(
                "File does not have a valid PE signature (MZ header not found)."
            );

        var peOffset = (int)ReadUInt32(fileBytes, 60);
        if (peOffset + 4 > fileBytes.Length || ReadUInt32(fileBytes, peOffset) != PeSignature)
            throw new InvalidDataException(
                "File does not have a valid PE signature (PE header not found)."
            );

        // COFF file header at peOffset + 4
        var coffHeaderOffset = peOffset + 4;
        if (coffHeaderOffset + 20 > fileBytes.Length)
            throw new InvalidDataException("PE COFF header is truncated.");

        var numberOfSections = (int)ReadUInt16(fileBytes, coffHeaderOffset + 2);
        var sizeOfOptionalHeader = (int)ReadUInt16(fileBytes, coffHeaderOffset + 16);

        // Optional header at coffHeaderOffset + 20
        var optHeaderOffset = coffHeaderOffset + 20;
        if (optHeaderOffset + 2 > fileBytes.Length)
            throw new InvalidDataException("PE optional header is missing.");

        var optMagic = ReadUInt16(fileBytes, optHeaderOffset);
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
        if (optHeaderOffset + minOptHeaderSize > fileBytes.Length)
            throw new InvalidDataException("PE optional header is truncated.");

        var sectionAlignment = ReadUInt32(fileBytes, optHeaderOffset + 32);
        var fileAlignment = ReadUInt32(fileBytes, optHeaderOffset + 36);
        var sizeOfImageFileOffset = optHeaderOffset + 56;
        var sizeOfHeadersValue = (int)ReadUInt32(fileBytes, optHeaderOffset + 60);

        // DataDirectory base: PE32 = optHeader + 96, PE32+ = optHeader + 112
        // DataDirectory[2] (Resource) is at base + 2 * 8 = base + 16
        // Validate that the file has room for the data directory entry
        var dataDirBase = optHeaderOffset + (isPe32Plus ? 112 : 96);
        if (dataDirBase + 24 > fileBytes.Length)
            throw new InvalidDataException(
                "PE optional header is too small to contain the resource data directory entry."
            );

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
            if (headerOffset + SectionHeaderSize > fileBytes.Length)
                break;

            var nameBytes = new byte[8];
            Array.Copy(fileBytes, headerOffset, nameBytes, 0, 8);
            var sectionName = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

            var section = new SectionInfo
            {
                Name = sectionName,
                VirtualSize = ReadUInt32(fileBytes, headerOffset + 8),
                VirtualAddress = ReadUInt32(fileBytes, headerOffset + 12),
                SizeOfRawData = ReadUInt32(fileBytes, headerOffset + 16),
                PointerToRawData = ReadUInt32(fileBytes, headerOffset + 20),
                Characteristics = ReadUInt32(fileBytes, headerOffset + 36),
                HeaderFileOffset = headerOffset,
            };

            info.Sections.Add(section);
            if (sectionName == ".rsrc")
                info.RsrcSectionIndex = i;
        }

        return info;
    }

    #endregion
}
