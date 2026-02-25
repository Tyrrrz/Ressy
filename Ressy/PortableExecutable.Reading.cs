using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy;

public sealed partial class PortableExecutable
{
    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        if (_info.RsrcSectionIndex < 0)
            return [];

        var rsrc = _info.Sections[_info.RsrcSectionIndex];
        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return [];

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)rsrc.PointerToRawData;
        var sectionSize = (int)rsrc.SizeOfRawData;

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var result = new List<ResourceIdentifier>();
        ReadIdentifiers(reader, sectionBase, sectionSize, 0, null, null, result);
        return result;
    }

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    public IReadOnlyList<Resource> GetResources()
    {
        if (_info.RsrcSectionIndex < 0)
            return [];

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        return ReadResourcesFromSection(reader, _info.Sections[_info.RsrcSectionIndex]);
    }

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        if (_info.RsrcSectionIndex < 0)
            return null;

        var rsrc = _info.Sections[_info.RsrcSectionIndex];
        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return null;

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var data = FindResourceData(
            reader,
            (int)rsrc.PointerToRawData,
            (int)rsrc.SizeOfRawData,
            rsrc,
            identifier,
            0
        );

        return data is not null ? new Resource(identifier, data) : null;
    }

    /// <summary>
    /// Gets the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    // Walks the .rsrc directory tree collecting only identifiers, without loading data bytes.
    private static void ReadIdentifiers(
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        int dirOffset,
        ResourceType? type,
        ResourceName? name,
        List<ResourceIdentifier> result
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            return;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                break;

            reader.BaseStream.Position = entryAbs;
            var nameField = reader.ReadUInt32();
            var dataField = reader.ReadUInt32();

            if ((dataField & SubdirectoryFlag) != 0)
            {
                var subdirOffset = (int)(dataField & ~SubdirectoryFlag);

                if (type is null)
                {
                    var t =
                        (nameField & NameStringFlag) != 0
                            ? ResourceType.FromString(
                                ReadSectionString(
                                    reader,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceType.FromCode((int)(nameField & 0xFFFF));
                    ReadIdentifiers(
                        reader,
                        sectionBase,
                        sectionSize,
                        subdirOffset,
                        t,
                        null,
                        result
                    );
                }
                else if (name is null)
                {
                    var n =
                        (nameField & NameStringFlag) != 0
                            ? ResourceName.FromString(
                                ReadSectionString(
                                    reader,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceName.FromCode((int)(nameField & 0xFFFF));
                    ReadIdentifiers(
                        reader,
                        sectionBase,
                        sectionSize,
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
                if (type is null || name is null)
                    continue;

                var langId = (int)(nameField & 0xFFFF);
                result.Add(new ResourceIdentifier(type, name, new Language(langId)));
            }
        }
    }

    // Walks the resource directory looking for the specific identifier; returns data or null.
    private static byte[]? FindResourceData(
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            return null;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                break;

            reader.BaseStream.Position = entryAbs;
            var nameField = reader.ReadUInt32();
            var dataField = reader.ReadUInt32();

            if ((dataField & SubdirectoryFlag) == 0)
                continue;

            var subdirOffset = (int)(dataField & ~SubdirectoryFlag);

            // Level 0: match type
            var entryType =
                (nameField & NameStringFlag) != 0
                    ? ResourceType.FromString(
                        ReadSectionString(
                            reader,
                            sectionBase,
                            sectionSize,
                            (int)(nameField & ~NameStringFlag)
                        )
                    )
                    : ResourceType.FromCode((int)(nameField & 0xFFFF));

            if (!entryType.Equals(target.Type))
                continue;

            var nameResult = FindResourceDataInNameDir(
                reader,
                sectionBase,
                sectionSize,
                rsrc,
                target,
                subdirOffset
            );
            if (nameResult is not null)
                return nameResult;
        }

        return null;
    }

    private static byte[]? FindResourceDataInNameDir(
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            return null;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                break;

            reader.BaseStream.Position = entryAbs;
            var nameField = reader.ReadUInt32();
            var dataField = reader.ReadUInt32();

            if ((dataField & SubdirectoryFlag) == 0)
                continue;

            var entryName =
                (nameField & NameStringFlag) != 0
                    ? ResourceName.FromString(
                        ReadSectionString(
                            reader,
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
                reader,
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
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        SectionInfo rsrc,
        ResourceIdentifier target,
        int dirOffset
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            return null;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                break;

            reader.BaseStream.Position = entryAbs;
            var nameField = reader.ReadUInt32();
            var dataField = reader.ReadUInt32();

            // Language entries point to data entries (no subdirectory flag expected)
            if ((dataField & SubdirectoryFlag) != 0)
                continue;

            var langId = (int)(nameField & 0xFFFF);
            if (langId != target.Language.Id)
                continue;

            var dataEntryAbs = sectionBase + (int)dataField;
            if (dataEntryAbs + 16 > reader.BaseStream.Length)
                continue;

            reader.BaseStream.Position = dataEntryAbs;
            var dataRva = reader.ReadUInt32();
            var dataSize = (int)reader.ReadUInt32();

            // Jump to the data's file offset in the stream and read it directly
            var dataFileOffset =
                (long)rsrc.PointerToRawData + (long)dataRva - (long)rsrc.VirtualAddress;

            if (
                dataFileOffset < 0
                || dataFileOffset > int.MaxValue
                || dataFileOffset + dataSize > reader.BaseStream.Length
            )
                continue;

            reader.BaseStream.Position = (long)dataFileOffset;
            return reader.ReadBytes(dataSize);
        }

        return null;
    }

    private static List<Resource> ReadResourcesFromSection(BinaryReader reader, SectionInfo rsrc)
    {
        var result = new List<Resource>();

        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return result;

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)rsrc.PointerToRawData;
        var sectionSize = (int)rsrc.SizeOfRawData;

        // Walk the 3-level resource directory tree: type -> name -> language -> data
        ReadDirectory(reader, sectionBase, sectionSize, rsrc, 0, null, null, result);

        return result;
    }

    private static void ReadDirectory(
        BinaryReader reader,
        int sectionBase, // file offset of the start of the .rsrc section
        int sectionSize,
        SectionInfo rsrc,
        int dirOffset, // offset within .rsrc section
        ResourceType? type,
        ResourceName? name,
        List<Resource> result
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            return;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                break;

            reader.BaseStream.Position = entryAbs;
            var nameField = reader.ReadUInt32();
            var dataField = reader.ReadUInt32();

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
                                    reader,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceType.FromCode((int)(nameField & 0xFFFF));
                    ReadDirectory(
                        reader,
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
                                    reader,
                                    sectionBase,
                                    sectionSize,
                                    (int)(nameField & ~NameStringFlag)
                                )
                            )
                            : ResourceName.FromCode((int)(nameField & 0xFFFF));
                    ReadDirectory(
                        reader,
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
                if (dataEntryAbs + 16 > reader.BaseStream.Length)
                    continue;

                reader.BaseStream.Position = dataEntryAbs;
                var dataRva = reader.ReadUInt32();
                var dataSize = (int)reader.ReadUInt32();

                // Jump to the data's file offset in the stream and read it directly
                var dataFileOffset =
                    (long)rsrc.PointerToRawData + (long)dataRva - (long)rsrc.VirtualAddress;

                if (
                    dataFileOffset < 0
                    || dataFileOffset > int.MaxValue
                    || dataFileOffset + dataSize > reader.BaseStream.Length
                )
                    continue;

                reader.BaseStream.Position = (long)dataFileOffset;
                var data = reader.ReadBytes(dataSize);

                result.Add(
                    new Resource(new ResourceIdentifier(type, name, new Language(langId)), data)
                );
            }
        }
    }

    private static string ReadSectionString(
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        int stringOffset
    )
    {
        var absOffset = sectionBase + stringOffset;
        if (absOffset + 2 > reader.BaseStream.Length)
            return "";

        reader.BaseStream.Position = absOffset;
        var charCount = (int)reader.ReadUInt16();
        var byteCount = charCount * 2;

        if (absOffset + 2 + byteCount > reader.BaseStream.Length)
            return "";

        return Encoding.Unicode.GetString(reader.ReadBytes(byteCount));
    }
}
