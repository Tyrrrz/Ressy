using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ressy.Utils;

namespace Ressy;

public partial class PortableExecutable
{
    // IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE
    private const uint RsrcSectionCharacteristics = 0xC0000040u;

    // Reads the full PE file from the stream, rebuilds the .rsrc section with the given resources,
    // writes the modified bytes back to the stream, and re-parses the PE metadata.
    private void UpdateResources(IReadOnlyDictionary<ResourceIdentifier, byte[]> resources)
    {
        if (_stream.Length > int.MaxValue)
            throw new InvalidDataException("PE file is too large to be processed.");

        _stream.Position = 0;
        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var fileBytes = reader.ReadBytes((int)_stream.Length);

        var info = _info;
        var rsrcList = resources.Select(kv => (kv.Key, kv.Value)).ToList();

        // Determine VirtualAddress for the new .rsrc section.
        // Keep the existing VA when the new aligned virtual size fits in the same allocation;
        // otherwise pick a new VA after all other sections.
        uint newVirtualAddress;

        if (info.RsrcSectionIndex >= 0)
        {
            var old = info.Sections[info.RsrcSectionIndex];
            var oldAligned = Arithmetic.AlignUp(old.VirtualSize, info.SectionAlignment);

            // Tentatively build with the old VA to measure the size
            var probe = BuildResourceSection(rsrcList, old.VirtualAddress);
            var newAligned = Arithmetic.AlignUp((uint)probe.Length, info.SectionAlignment);

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
        var alignedSize = Arithmetic.AlignUp(newRsrcContent.Length, (int)info.FileAlignment);
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
                {
                    Array.Clear(
                        newFileBytes,
                        (int)old.PointerToRawData + alignedSize,
                        (int)old.SizeOfRawData - alignedSize
                    );
                }
                filePosition = (int)old.PointerToRawData;
            }
            else
            {
                // Append: new data is larger than the existing raw allocation
                filePosition = Arithmetic.AlignUp(fileBytes.Length, (int)info.FileAlignment);
                newFileBytes = new byte[filePosition + alignedSize];
                Array.Copy(fileBytes, newFileBytes, fileBytes.Length);
                Array.Copy(alignedRsrcContent, 0, newFileBytes, filePosition, alignedSize);
            }

            // Update the .rsrc section header fields
            using var ms = new MemoryStream(newFileBytes);
            using var writer = new BinaryWriter(ms);
            var h = old.HeaderFileOffset;
            ms.Position = h + 8;
            writer.Write((uint)newRsrcContent.Length); // VirtualSize
            writer.Write(newVirtualAddress); // VirtualAddress
            writer.Write((uint)alignedSize); // SizeOfRawData
            writer.Write((uint)filePosition); // PointerToRawData
        }
        else
        {
            // No existing .rsrc section – append data and add a new section header entry
            filePosition = Arithmetic.AlignUp(fileBytes.Length, (int)info.FileAlignment);
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
            {
                throw new InvalidOperationException(
                    "Not enough space in the PE header area to add a .rsrc section. "
                        + "The file cannot be modified."
                );
            }

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
            {
                throw new InvalidOperationException(
                    "Cannot add a new section: the file already has the maximum number of sections."
                );
            }

            using var ms = new MemoryStream(newFileBytes);
            using var writer = new BinaryWriter(ms);
            ms.Position = info.NumberOfSectionsFileOffset;
            writer.Write((ushort)newSectionCount);
        }

        // Update DataDirectory[2] (Resource) and SizeOfImage
        {
            using var ms = new MemoryStream(newFileBytes);
            using var bwReader = new BinaryReader(ms);
            using var bwWriter = new BinaryWriter(ms);

            ms.Position = info.DataDir2FileOffset;
            bwWriter.Write(newVirtualAddress);
            bwWriter.Write((uint)newRsrcContent.Length);

            // Update SizeOfImage if the new section extends beyond the current image size
            var requiredSizeOfImage = Arithmetic.AlignUp(
                newVirtualAddress
                    + Arithmetic.AlignUp((uint)newRsrcContent.Length, info.SectionAlignment),
                info.SectionAlignment
            );
            ms.Position = info.SizeOfImageFileOffset;
            var currentSizeOfImage = bwReader.ReadUInt32();
            if (requiredSizeOfImage > currentSizeOfImage)
            {
                ms.Position = info.SizeOfImageFileOffset;
                bwWriter.Write(requiredSizeOfImage);
            }
        }

        // Write modified bytes back to the stream
        _stream.Position = 0;
        _stream.Write(newFileBytes, 0, newFileBytes.Length);
        _stream.SetLength(newFileBytes.Length);
        _stream.Flush();

        // Re-parse PE metadata since the structure may have changed
        _info = ParsePEInfo(_stream);
    }

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
        offset = Arithmetic.AlignUp(offset, 4);
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
                    offset = Arithmetic.AlignUp(offset + langs[li].Data.Length, 4);
                }
            }
        }

        // ── Serialization ───────────────────────────────────────────────────────────

        var result = new byte[offset];
        using var ms = new MemoryStream(result);
        using var writer = new BinaryWriter(ms);

        // Root directory
        var namedTypeCount = byType.Count(g => g.Key.Code is null);
        WriteDirectory(writer, 0, namedTypeCount, byType.Count - namedTypeCount);

        for (var ti = 0; ti < byType.Count; ti++)
        {
            var entryOffset = 16 + ti * 8;
            WriteTypeEntryId(writer, entryOffset, byType[ti].Key, namedStrings);
            ms.Position = entryOffset + 4;
            writer.Write((uint)typeDirOffsets[ti] | SubdirectoryFlag);
        }

        // Type directories
        for (var ti = 0; ti < byType.Count; ti++)
        {
            var nameGroups = GetSortedNameGroups(byType[ti]);
            var namedNameCount = nameGroups.Count(g => g.Key.Code is null);
            WriteDirectory(
                writer,
                typeDirOffsets[ti],
                namedNameCount,
                nameGroups.Count - namedNameCount
            );

            for (var ni = 0; ni < nameGroups.Count; ni++)
            {
                var entryOffset = typeDirOffsets[ti] + 16 + ni * 8;
                WriteNameEntryId(writer, entryOffset, nameGroups[ni].Key, namedStrings);
                ms.Position = entryOffset + 4;
                writer.Write((uint)nameDirOffsets[ti][ni] | SubdirectoryFlag);
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
                WriteDirectory(writer, nameDirOffsets[ti][ni], 0, langs.Count);

                for (var li = 0; li < langs.Count; li++)
                {
                    var entryOffset = nameDirOffsets[ti][ni] + 16 + li * 8;
                    ms.Position = entryOffset;
                    writer.Write((uint)(langs[li].Lang.Id & 0xFFFF));
                    // Points to data entry (no subdirectory flag)
                    writer.Write((uint)dataEntryOffsets[ti][ni][li]);
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

                    ms.Position = de;
                    // OffsetToData is an RVA: sectionVA + offset-within-section
                    writer.Write(sectionVirtualAddress + (uint)dataOffsetInSection);
                    writer.Write((uint)data.Length);
                    writer.Write(0u); // CodePage
                    writer.Write(0u); // Reserved
                }
            }
        }

        // Name strings (IMAGE_RESOURCE_DIR_STRING_U)
        foreach (var (str, strOffset) in namedStrings)
        {
            ms.Position = strOffset;
            writer.Write((ushort)str.Length);
            writer.Write(Encoding.Unicode.GetBytes(str));
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
                    ms.Position = resourceDataOffsets[ti][ni][li];
                    writer.Write(langs[li].Data);
                }
            }
        }

        return result;
    }

    private static void WriteDirectory(
        BinaryWriter writer,
        int offset,
        int namedEntries,
        int idEntries
    )
    {
        // Characteristics, TimeDateStamp, MajorVersion, MinorVersion are all zero
        writer.BaseStream.Position = offset + 12;
        writer.Write((ushort)namedEntries);
        writer.Write((ushort)idEntries);
    }

    private static void WriteTypeEntryId(
        BinaryWriter writer,
        int offset,
        ResourceType type,
        Dictionary<string, int> namedStrings
    )
    {
        writer.BaseStream.Position = offset;
        if (type.Code is not null)
            writer.Write((uint)(type.Code.Value & 0xFFFF));
        else
            writer.Write((uint)namedStrings[type.Label] | NameStringFlag);
    }

    private static void WriteNameEntryId(
        BinaryWriter writer,
        int offset,
        ResourceName name,
        Dictionary<string, int> namedStrings
    )
    {
        writer.BaseStream.Position = offset;
        if (name.Code is not null)
            writer.Write((uint)(name.Code.Value & 0xFFFF));
        else
            writer.Write((uint)namedStrings[name.Label] | NameStringFlag);
    }

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

    private static uint FindNextVirtualAddress(PEInfo info, bool excludeRsrc)
    {
        uint maxEnd = 0;
        for (var i = 0; i < info.Sections.Count; i++)
        {
            if (excludeRsrc && i == info.RsrcSectionIndex)
                continue;
            var s = info.Sections[i];
            var end = Arithmetic.AlignUp(s.VirtualAddress + s.VirtualSize, info.SectionAlignment);
            if (end > maxEnd)
                maxEnd = end;
        }

        return Arithmetic.AlignUp(maxEnd, info.SectionAlignment);
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
        using var ms = new MemoryStream(buf);
        using var writer = new BinaryWriter(ms);

        ms.Position = offset;
        var nameBytes = new byte[8];
        var encoded = Encoding.ASCII.GetBytes(name);
        Array.Copy(encoded, nameBytes, Math.Min(encoded.Length, 8));
        writer.Write(nameBytes);

        writer.Write(virtualSize);
        writer.Write(virtualAddress);
        writer.Write(sizeOfRawData);
        writer.Write(pointerToRawData);
        // PointerToRelocations = 0, PointerToLineNumbers = 0,
        // NumberOfRelocations = 0, NumberOfLineNumbers = 0 (already zero from allocation)

        ms.Position = offset + 36;
        writer.Write(characteristics);
    }
}
