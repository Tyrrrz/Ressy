using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy.PE;

internal static partial class PeFile
{
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
}
