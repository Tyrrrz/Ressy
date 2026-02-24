using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy.PE;

// Provides cross-platform reading and writing of native resources in PE (Portable Executable) files.
internal static class PeFile
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

    #region Resource reading

    public static List<(ResourceIdentifier Id, byte[] Data)> ReadResources(string filePath)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var info = ParsePeInfo(fileBytes);

        if (info.RsrcSectionIndex < 0)
            return new List<(ResourceIdentifier, byte[])>();

        return ReadResourcesFromSection(fileBytes, info.Sections[info.RsrcSectionIndex]);
    }

    // Reads only the single resource matching the given identifier; returns null if not found.
    public static byte[]? TryReadResource(string filePath, ResourceIdentifier identifier)
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var info = ParsePeInfo(fileBytes);

        if (info.RsrcSectionIndex < 0)
            return null;

        var rsrc = info.Sections[info.RsrcSectionIndex];

        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return null;

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)rsrc.PointerToRawData;
        var sectionSize = (int)rsrc.SizeOfRawData;

        return FindResourceData(fileBytes, sectionBase, sectionSize, rsrc, identifier, 0);
    }

    // Walks the resource directory looking for the specific identifier; returns data or null.
    private static byte[]? FindResourceData(
        byte[] fileBytes,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > fileBytes.Length)
            return null;

        var numNamed = (int)ReadUInt16(fileBytes, absOffset + 12);
        var numId = (int)ReadUInt16(fileBytes, absOffset + 14);
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > fileBytes.Length)
                break;

            var nameField = ReadUInt32(fileBytes, entryAbs);
            var dataField = ReadUInt32(fileBytes, entryAbs + 4);

            if ((dataField & SubdirectoryFlag) != 0)
            {
                var subdirOffset = (int)(dataField & ~SubdirectoryFlag);

                // Level 0: match type
                var entryType =
                    (nameField & NameStringFlag) != 0
                        ? ResourceType.FromString(
                            ReadSectionString(
                                fileBytes,
                                sectionBase,
                                sectionSize,
                                (int)(nameField & ~NameStringFlag)
                            )
                        )
                        : ResourceType.FromCode((int)(nameField & 0xFFFF));

                if (!entryType.Equals(target.Type))
                    continue;

                // Drill into the matching type subdirectory
                var nameResult = FindResourceDataInNameDir(
                    fileBytes,
                    sectionBase,
                    sectionSize,
                    rsrc,
                    target,
                    subdirOffset
                );
                if (nameResult is not null)
                    return nameResult;
            }
        }

        return null;
    }

    private static byte[]? FindResourceDataInNameDir(
        byte[] fileBytes,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > fileBytes.Length)
            return null;

        var numNamed = (int)ReadUInt16(fileBytes, absOffset + 12);
        var numId = (int)ReadUInt16(fileBytes, absOffset + 14);
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > fileBytes.Length)
                break;

            var nameField = ReadUInt32(fileBytes, entryAbs);
            var dataField = ReadUInt32(fileBytes, entryAbs + 4);

            if ((dataField & SubdirectoryFlag) == 0)
                continue;

            var entryName =
                (nameField & NameStringFlag) != 0
                    ? ResourceName.FromString(
                        ReadSectionString(
                            fileBytes,
                            sectionBase,
                            sectionSize,
                            (int)(nameField & ~NameStringFlag)
                        )
                    )
                    : ResourceName.FromCode((int)(nameField & 0xFFFF));

            if (!entryName.Equals(target.Name))
                continue;

            var subdirOffset = (int)(dataField & ~SubdirectoryFlag);
            return FindResourceDataInLangDir(
                fileBytes,
                sectionBase,
                sectionSize,
                rsrc,
                target,
                subdirOffset
            );
        }

        return null;
    }

    private static byte[]? FindResourceDataInLangDir(
        byte[] fileBytes,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > fileBytes.Length)
            return null;

        var numNamed = (int)ReadUInt16(fileBytes, absOffset + 12);
        var numId = (int)ReadUInt16(fileBytes, absOffset + 14);
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > fileBytes.Length)
                break;

            var nameField = ReadUInt32(fileBytes, entryAbs);
            var dataField = ReadUInt32(fileBytes, entryAbs + 4);

            // Language entries point to data entries (no subdirectory flag expected)
            if ((dataField & SubdirectoryFlag) != 0)
                continue;

            var langId = (int)(nameField & 0xFFFF);
            if (langId != target.Language.Id)
                continue;

            var dataEntryAbs = sectionBase + (int)dataField;
            if (dataEntryAbs + 16 > fileBytes.Length)
                continue;

            var dataRva = ReadUInt32(fileBytes, dataEntryAbs);
            var dataSize = (int)ReadUInt32(fileBytes, dataEntryAbs + 4);

            var dataFileOffset =
                (long)rsrc.PointerToRawData + (long)dataRva - (long)rsrc.VirtualAddress;

            if (
                dataFileOffset < 0
                || dataFileOffset > int.MaxValue
                || dataFileOffset + dataSize > fileBytes.Length
            )
                continue;

            var data = new byte[dataSize];
            Array.Copy(fileBytes, (int)dataFileOffset, data, 0, dataSize);
            return data;
        }

        return null;
    }

    private static List<(ResourceIdentifier Id, byte[] Data)> ReadResourcesFromSection(
        byte[] fileBytes,
        SectionInfo rsrc
    )
    {
        var result = new List<(ResourceIdentifier, byte[])>();

        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return result;

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)rsrc.PointerToRawData;
        var sectionSize = (int)rsrc.SizeOfRawData;

        // Walk the 3-level resource directory tree: type -> name -> language -> data
        ReadDirectory(fileBytes, sectionBase, sectionSize, rsrc, 0, null, null, result);

        return result;
    }

    private static void ReadDirectory(
        byte[] fileBytes,
        int sectionBase, // file offset of the start of the .rsrc section
        int sectionSize,
        SectionInfo rsrc,
        int dirOffset, // offset within .rsrc section
        ResourceType? type,
        ResourceName? name,
        List<(ResourceIdentifier, byte[])> result
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > fileBytes.Length)
            return;

        var numNamed = (int)ReadUInt16(fileBytes, absOffset + 12);
        var numId = (int)ReadUInt16(fileBytes, absOffset + 14);
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > fileBytes.Length)
                break;

            var nameField = ReadUInt32(fileBytes, entryAbs);
            var dataField = ReadUInt32(fileBytes, entryAbs + 4);

            if ((dataField & SubdirectoryFlag) != 0)
            {
                var subdirOffset = (int)(dataField & ~SubdirectoryFlag);

                if (type is null)
                {
                    // Level 0 → resolve resource type
                    var t =
                        (nameField & NameStringFlag) != 0
                            ? ResourceType.FromString(
                                ReadSectionString(
                                    fileBytes,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceType.FromCode((int)(nameField & 0xFFFF));
                    ReadDirectory(
                        fileBytes,
                        sectionBase,
                        sectionSize,
                        rsrc,
                        subdirOffset,
                        t,
                        null,
                        result
                    );
                }
                else if (name is null)
                {
                    // Level 1 → resolve resource name
                    var n =
                        (nameField & NameStringFlag) != 0
                            ? ResourceName.FromString(
                                ReadSectionString(
                                    fileBytes,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceName.FromCode((int)(nameField & 0xFFFF));
                    ReadDirectory(
                        fileBytes,
                        sectionBase,
                        sectionSize,
                        rsrc,
                        subdirOffset,
                        type,
                        n,
                        result
                    );
                }
                // Level 2 (language): unexpected subdirectory, skip
            }
            else
            {
                // Leaf: points to IMAGE_RESOURCE_DATA_ENTRY
                if (type is null || name is null)
                    continue;

                var langId = (int)(nameField & 0xFFFF);
                var dataEntryAbs = sectionBase + (int)dataField;
                if (dataEntryAbs + 16 > fileBytes.Length)
                    continue;

                var dataRva = ReadUInt32(fileBytes, dataEntryAbs);
                var dataSize = (int)ReadUInt32(fileBytes, dataEntryAbs + 4);

                // Convert RVA to file offset via the section header
                var dataFileOffset =
                    (long)rsrc.PointerToRawData + (long)dataRva - (long)rsrc.VirtualAddress;

                if (
                    dataFileOffset < 0
                    || dataFileOffset > int.MaxValue
                    || dataFileOffset + dataSize > fileBytes.Length
                )
                    continue;

                var data = new byte[dataSize];
                Array.Copy(fileBytes, (int)dataFileOffset, data, 0, dataSize);

                result.Add((new ResourceIdentifier(type, name, new Language(langId)), data));
            }
        }
    }

    private static string ReadSectionString(
        byte[] fileBytes,
        int sectionBase,
        int sectionSize,
        int stringOffset
    )
    {
        var absOffset = sectionBase + stringOffset;
        if (absOffset + 2 > fileBytes.Length)
            return "";

        var charCount = (int)ReadUInt16(fileBytes, absOffset);
        var byteCount = charCount * 2;

        if (absOffset + 2 + byteCount > fileBytes.Length)
            return "";

        return Encoding.Unicode.GetString(fileBytes, absOffset + 2, byteCount);
    }

    #endregion

    #region Resource section building

    // Builds the binary content of a .rsrc section for the given list of resources.
    // sectionVirtualAddress: the RVA at which the section will be loaded.
    private static byte[] BuildResourceSection(
        IReadOnlyList<(ResourceIdentifier Id, byte[] Data)> resources,
        uint sectionVirtualAddress
    )
    {
        if (resources.Count == 0)
        {
            // An empty root directory: 16 bytes header, zero entries
            return new byte[16];
        }

        // Group and sort: named types first (Code is null), then IDs ascending
        var byType = resources
            .GroupBy(r => r.Id.Type, ResourceTypeComparer.Instance)
            .OrderBy(g => g.Key.Code.HasValue ? 1 : 0)
            .ThenBy(g => g.Key.Code ?? int.MaxValue)
            .ThenBy(g => g.Key.Code is null ? g.Key.Label : "")
            .ToList();

        // Collect unique named strings (for both types and names)
        // Key = label string, Value = assigned offset within the section (filled in layout phase)
        var namedStrings = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tg in byType)
        {
            if (tg.Key.Code is null)
                namedStrings.TryAdd(tg.Key.Label, 0);

            foreach (var ng in tg.GroupBy(r => r.Id.Name, ResourceNameComparer.Instance))
            {
                if (ng.Key.Code is null)
                    namedStrings.TryAdd(ng.Key.Label, 0);
            }
        }

        // ── Layout computation ──────────────────────────────────────────────────────

        // Level-0 root directory
        var rootDirSize = 16 + byType.Count * 8;

        // Level-1 type directories
        var typeDirOffsets = new int[byType.Count];
        var offset = rootDirSize;
        for (var ti = 0; ti < byType.Count; ti++)
        {
            typeDirOffsets[ti] = offset;
            offset += 16 + GetSortedNameGroups(byType[ti]).Count * 8;
        }

        // Level-2 name directories
        var nameDirOffsets = new int[byType.Count][];
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            nameDirOffsets[ti] = new int[nameGroups.Count];
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                nameDirOffsets[ti][ni] = offset;
                offset += 16 + GetSortedLanguageEntries(nameGroups[ni]).Count * 8;
            }
        }

        // Data entries (IMAGE_RESOURCE_DATA_ENTRY, 16 bytes each)
        var dataEntryOffsets = new int[byType.Count][][];
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            dataEntryOffsets[ti] = new int[nameGroups.Count][];
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var langs = GetSortedLanguageEntries(nameGroups[ni]);
                dataEntryOffsets[ti][ni] = new int[langs.Count];
                for (var li = 0; li < langs.Count; li++)
                {
                    dataEntryOffsets[ti][ni][li] = offset;
                    offset += 16;
                }
            }
        }

        // Name strings
        {
            var keys = namedStrings.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
            foreach (var key in keys)
            {
                namedStrings[key] = offset;
                offset += 2 + key.Length * 2; // WORD length + UTF-16 chars (no null terminator)
            }
        }

        // Resource data (DWORD-aligned between entries)
        offset = AlignUp(offset, 4);
        var resourceDataOffsets = new int[byType.Count][][];
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            resourceDataOffsets[ti] = new int[nameGroups.Count][];
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var langs = GetSortedLanguageEntries(nameGroups[ni]);
                resourceDataOffsets[ti][ni] = new int[langs.Count];
                for (var li = 0; li < langs.Count; li++)
                {
                    resourceDataOffsets[ti][ni][li] = offset;
                    offset = AlignUp(offset + langs[li].Data.Length, 4);
                }
            }
        }

        // ── Serialization ───────────────────────────────────────────────────────────

        var result = new byte[offset];

        // Root directory
        var namedTypeCount = byType.Count(g => g.Key.Code is null);
        WriteDirectory(result, 0, namedTypeCount, byType.Count - namedTypeCount);

        for (var ti = 0; ti < byType.Count; ti++)
        {
            var entryOffset = 16 + ti * 8;
            WriteTypeEntryId(result, entryOffset, byType[ti].Key, namedStrings);
            WriteUInt32(result, entryOffset + 4, (uint)typeDirOffsets[ti] | SubdirectoryFlag);
        }

        // Type directories
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            var namedNameCount = nameGroups.Count(g => g.Key.Code is null);
            WriteDirectory(
                result,
                typeDirOffsets[ti],
                namedNameCount,
                nameGroups.Count - namedNameCount
            );

            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var entryOffset = typeDirOffsets[ti] + 16 + ni * 8;
                WriteNameEntryId(result, entryOffset, nameGroups[ni].Key, namedStrings);
                WriteUInt32(
                    result,
                    entryOffset + 4,
                    (uint)nameDirOffsets[ti][ni] | SubdirectoryFlag
                );
            }
        }

        // Name directories (language entries)
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var langs = GetSortedLanguageEntries(nameGroups[ni]);
                // All language entries are IDs (no named languages)
                WriteDirectory(result, nameDirOffsets[ti][ni], 0, langs.Count);

                for (var li = 0; li < langs.Count; li++)
                {
                    var entryOffset = nameDirOffsets[ti][ni] + 16 + li * 8;
                    WriteUInt32(result, entryOffset, (uint)(langs[li].Lang.Id & 0xFFFF));
                    // Points to data entry (no subdirectory flag)
                    WriteUInt32(result, entryOffset + 4, (uint)dataEntryOffsets[ti][ni][li]);
                }
            }
        }

        // Data entries (IMAGE_RESOURCE_DATA_ENTRY)
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var langs = GetSortedLanguageEntries(nameGroups[ni]);
                for (var li = 0; li < langs.Count; li++)
                {
                    var de = dataEntryOffsets[ti][ni][li];
                    var data = langs[li].Data;
                    var dataOffsetInSection = resourceDataOffsets[ti][ni][li];

                    // OffsetToData is an RVA: sectionVA + offset-within-section
                    WriteUInt32(result, de, sectionVirtualAddress + (uint)dataOffsetInSection);
                    WriteUInt32(result, de + 4, (uint)data.Length);
                    WriteUInt32(result, de + 8, 0); // CodePage
                    WriteUInt32(result, de + 12, 0); // Reserved
                }
            }
        }

        // Name strings (IMAGE_RESOURCE_DIR_STRING_U)
        foreach (var (str, strOffset) in namedStrings)
        {
            WriteUInt16(result, strOffset, (ushort)str.Length);
            var strBytes = Encoding.Unicode.GetBytes(str);
            Array.Copy(strBytes, 0, result, strOffset + 2, strBytes.Length);
        }

        // Resource data
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var langs = GetSortedLanguageEntries(nameGroups[ni]);
                for (var li = 0; li < langs.Count; li++)
                {
                    var data = langs[li].Data;
                    Array.Copy(data, 0, result, resourceDataOffsets[ti][ni][li], data.Length);
                }
            }
        }

        return result;
    }

    private static void WriteDirectory(byte[] buf, int offset, int namedEntries, int idEntries)
    {
        // Characteristics, TimeDateStamp, MajorVersion, MinorVersion are all zero
        WriteUInt16(buf, offset + 12, (ushort)namedEntries);
        WriteUInt16(buf, offset + 14, (ushort)idEntries);
    }

    private static void WriteTypeEntryId(
        byte[] buf,
        int offset,
        ResourceType type,
        Dictionary<string, int> namedStrings
    )
    {
        if (type.Code is not null)
            WriteUInt32(buf, offset, (uint)(type.Code.Value & 0xFFFF));
        else
            WriteUInt32(buf, offset, (uint)namedStrings[type.Label] | NameStringFlag);
    }

    private static void WriteNameEntryId(
        byte[] buf,
        int offset,
        ResourceName name,
        Dictionary<string, int> namedStrings
    )
    {
        if (name.Code is not null)
            WriteUInt32(buf, offset, (uint)(name.Code.Value & 0xFFFF));
        else
            WriteUInt32(buf, offset, (uint)namedStrings[name.Label] | NameStringFlag);
    }

    #endregion

    #region Resource tree helpers

    private sealed class ResourceTypeComparer : IEqualityComparer<ResourceType>
    {
        public static readonly ResourceTypeComparer Instance = new();

        public bool Equals(ResourceType? x, ResourceType? y) => x?.Equals(y) ?? y is null;

        public int GetHashCode(ResourceType obj) => obj.GetHashCode();
    }

    private sealed class ResourceNameComparer : IEqualityComparer<ResourceName>
    {
        public static readonly ResourceNameComparer Instance = new();

        public bool Equals(ResourceName? x, ResourceName? y) => x?.Equals(y) ?? y is null;

        public int GetHashCode(ResourceName obj) => obj.GetHashCode();
    }

    private static List<
        IGrouping<ResourceName, (ResourceIdentifier Id, byte[] Data)>
    > GetSortedNameGroups(
        IGrouping<ResourceType, (ResourceIdentifier Id, byte[] Data)> typeGroup
    ) =>
        typeGroup
            .GroupBy(r => r.Id.Name, ResourceNameComparer.Instance)
            .OrderBy(g => g.Key.Code.HasValue ? 1 : 0)
            .ThenBy(g => g.Key.Code ?? int.MaxValue)
            .ThenBy(g => g.Key.Code is null ? g.Key.Label : "")
            .ToList();

    private static List<(Language Lang, byte[] Data)> GetSortedLanguageEntries(
        IGrouping<ResourceName, (ResourceIdentifier Id, byte[] Data)> nameGroup
    ) => nameGroup.OrderBy(r => r.Id.Language.Id).Select(r => (r.Id.Language, r.Data)).ToList();

    #endregion

    #region PE file updating

    public static void UpdateResources(
        string filePath,
        Action<Dictionary<ResourceIdentifier, byte[]>> modify,
        bool deleteExisting = false
    )
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var info = ParsePeInfo(fileBytes);

        // Load existing resources into a mutable dictionary
        var resources = new Dictionary<ResourceIdentifier, byte[]>();

        if (!deleteExisting && info.RsrcSectionIndex >= 0)
        {
            foreach (
                var (id, data) in ReadResourcesFromSection(
                    fileBytes,
                    info.Sections[info.RsrcSectionIndex]
                )
            )
            {
                resources[id] = data;
            }
        }

        // Apply caller modifications
        modify(resources);

        var rsrcList = resources.Select(kv => (kv.Key, kv.Value)).ToList();

        // Determine VirtualAddress for the new .rsrc section.
        // Keep the existing VA when the new aligned virtual size fits in the same allocation;
        // otherwise pick a new VA after all other sections.
        uint newVirtualAddress;

        if (info.RsrcSectionIndex >= 0)
        {
            var old = info.Sections[info.RsrcSectionIndex];
            var oldAligned = AlignUp(old.VirtualSize, info.SectionAlignment);

            // Tentatively build with the old VA to measure the size
            var probe = BuildResourceSection(rsrcList, old.VirtualAddress);
            var newAligned = AlignUp((uint)probe.Length, info.SectionAlignment);

            newVirtualAddress =
                newAligned <= oldAligned
                    ? old.VirtualAddress
                    : FindNextVirtualAddress(info, excludeRsrc: true);
        }
        else
        {
            newVirtualAddress = FindNextVirtualAddress(info, excludeRsrc: false);
        }

        var newRsrcContent = BuildResourceSection(rsrcList, newVirtualAddress);

        // Pad to FileAlignment
        var alignedSize = AlignUp(newRsrcContent.Length, (int)info.FileAlignment);
        var alignedRsrcContent = new byte[alignedSize];
        Array.Copy(newRsrcContent, alignedRsrcContent, newRsrcContent.Length);

        byte[] newFileBytes;
        int filePosition;

        if (info.RsrcSectionIndex >= 0)
        {
            var old = info.Sections[info.RsrcSectionIndex];

            if (alignedSize <= (int)old.SizeOfRawData)
            {
                // In-place: new data fits in the existing raw allocation
                newFileBytes = (byte[])fileBytes.Clone();
                Array.Copy(
                    alignedRsrcContent,
                    0,
                    newFileBytes,
                    (int)old.PointerToRawData,
                    alignedSize
                );
                // Zero remainder
                if (alignedSize < (int)old.SizeOfRawData)
                    Array.Clear(
                        newFileBytes,
                        (int)old.PointerToRawData + alignedSize,
                        (int)old.SizeOfRawData - alignedSize
                    );
                filePosition = (int)old.PointerToRawData;
            }
            else
            {
                // Append: new data is larger than the existing raw allocation
                filePosition = AlignUp(fileBytes.Length, (int)info.FileAlignment);
                newFileBytes = new byte[filePosition + alignedSize];
                Array.Copy(fileBytes, newFileBytes, fileBytes.Length);
                Array.Copy(alignedRsrcContent, 0, newFileBytes, filePosition, alignedSize);
            }

            // Update the .rsrc section header fields
            var h = old.HeaderFileOffset;
            WriteUInt32(newFileBytes, h + 8, (uint)newRsrcContent.Length); // VirtualSize
            WriteUInt32(newFileBytes, h + 12, newVirtualAddress); // VirtualAddress
            WriteUInt32(newFileBytes, h + 16, (uint)alignedSize); // SizeOfRawData
            WriteUInt32(newFileBytes, h + 20, (uint)filePosition); // PointerToRawData
        }
        else
        {
            // No existing .rsrc section – append data and add a new section header entry
            filePosition = AlignUp(fileBytes.Length, (int)info.FileAlignment);
            newFileBytes = new byte[filePosition + alignedSize];
            Array.Copy(fileBytes, newFileBytes, fileBytes.Length);
            Array.Copy(alignedRsrcContent, 0, newFileBytes, filePosition, alignedSize);

            var newHeaderOffset =
                info.FirstSectionHeaderFileOffset + info.Sections.Count * SectionHeaderSize;

            // Verify there is room for the new section header by checking against the
            // first section's raw data offset (the real end of the headers area)
            var firstSectionStart =
                info.Sections.Count > 0
                    ? (int)info.Sections.Min(s => s.PointerToRawData)
                    : info.SizeOfHeadersValue;

            if (newHeaderOffset + SectionHeaderSize > firstSectionStart)
                throw new InvalidOperationException(
                    "Not enough space in the PE header area to add a .rsrc section. "
                        + "The file cannot be modified."
                );
            WriteSectionHeader(
                newFileBytes,
                newHeaderOffset,
                ".rsrc",
                (uint)newRsrcContent.Length,
                newVirtualAddress,
                (uint)alignedSize,
                (uint)filePosition,
                RsrcSectionCharacteristics
            );

            var newSectionCount = info.Sections.Count + 1;
            if (newSectionCount > ushort.MaxValue)
                throw new InvalidOperationException(
                    "Cannot add a new section: the file already has the maximum number of sections."
                );

            WriteUInt16(newFileBytes, info.NumberOfSectionsFileOffset, (ushort)newSectionCount);
        }

        // Update DataDirectory[2] (Resource)
        WriteUInt32(newFileBytes, info.DataDir2FileOffset, newVirtualAddress);
        WriteUInt32(newFileBytes, info.DataDir2FileOffset + 4, (uint)newRsrcContent.Length);

        // Update SizeOfImage if the new section extends beyond the current image size
        var requiredSizeOfImage = AlignUp(
            newVirtualAddress + AlignUp((uint)newRsrcContent.Length, info.SectionAlignment),
            info.SectionAlignment
        );
        var currentSizeOfImage = ReadUInt32(newFileBytes, info.SizeOfImageFileOffset);
        if (requiredSizeOfImage > currentSizeOfImage)
            WriteUInt32(newFileBytes, info.SizeOfImageFileOffset, requiredSizeOfImage);

        File.WriteAllBytes(filePath, newFileBytes);
    }

    private static uint FindNextVirtualAddress(PeInfo info, bool excludeRsrc)
    {
        uint maxEnd = 0;
        for (var i = 0; i < info.Sections.Count; i++)
        {
            if (excludeRsrc && i == info.RsrcSectionIndex)
                continue;
            var s = info.Sections[i];
            var end = AlignUp(s.VirtualAddress + s.VirtualSize, info.SectionAlignment);
            if (end > maxEnd)
                maxEnd = end;
        }

        return AlignUp(maxEnd, info.SectionAlignment);
    }

    private static void WriteSectionHeader(
        byte[] buf,
        int offset,
        string name,
        uint virtualSize,
        uint virtualAddress,
        uint sizeOfRawData,
        uint pointerToRawData,
        uint characteristics
    )
    {
        var nameBytes = new byte[8];
        var encoded = Encoding.ASCII.GetBytes(name);
        Array.Copy(encoded, nameBytes, Math.Min(encoded.Length, 8));
        Array.Copy(nameBytes, 0, buf, offset, 8);

        WriteUInt32(buf, offset + 8, virtualSize);
        WriteUInt32(buf, offset + 12, virtualAddress);
        WriteUInt32(buf, offset + 16, sizeOfRawData);
        WriteUInt32(buf, offset + 20, pointerToRawData);
        // PointerToRelocations = 0, PointerToLineNumbers = 0,
        // NumberOfRelocations = 0, NumberOfLineNumbers = 0 (already zero from allocation)
        WriteUInt32(buf, offset + 36, characteristics);
    }

    #endregion
}
