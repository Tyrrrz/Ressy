using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy.PE;

internal static partial class PeFile
{
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
            .GroupBy(r => r.Id.Type)
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

            foreach (var ng in tg.GroupBy(r => r.Id.Name))
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

    private static List<
        IGrouping<ResourceName, (ResourceIdentifier Id, byte[] Data)>
    > GetSortedNameGroups(
        IGrouping<ResourceType, (ResourceIdentifier Id, byte[] Data)> typeGroup
    ) =>
        typeGroup
            .GroupBy(r => r.Id.Name)
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
        IReadOnlyDictionary<ResourceIdentifier, byte[]> resources
    )
    {
        var fileBytes = File.ReadAllBytes(filePath);
        var info = ParsePeInfo(fileBytes);

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
