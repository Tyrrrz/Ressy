using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy;

public partial class PortableExecutable
{
    // PE format constants
    private const ushort DosMagic = 0x5A4D; // 'MZ'
    private const uint PESignature = 0x4550; // 'PE\0\0'
    private const ushort PE32Magic = 0x10B;
    private const ushort PE32PlusMagic = 0x20B;
    private const uint NameStringFlag = 0x80000000u;
    private const uint SubdirectoryFlag = 0x80000000u;
    private const int SectionHeaderSize = 40;

    private sealed class PEInfo
    {
        public required bool IsPE32Plus { get; init; }
        public required uint SectionAlignment { get; init; }
        public required uint FileAlignment { get; init; }
        public required int DataDir2FileOffset { get; init; }
        public required int SizeOfImageFileOffset { get; init; }
        public required int NumberOfSectionsFileOffset { get; init; }
        public required int FirstSectionHeaderFileOffset { get; init; }
        public required int SizeOfHeadersValue { get; init; }
        public List<SectionInfo> Sections { get; } = [];
        public int ResourceSectionIndex { get; set; } = -1;
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

    private static PEInfo ParsePEInfo(Stream stream)
    {
        if (stream.Length < 64)
            throw new InvalidDataException("File is too small to be a valid PE file.");

        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

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
        if (reader.ReadUInt32() != PESignature)
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
        var isPE32Plus = optMagic switch
        {
            PE32Magic => false,
            PE32PlusMagic => true,
            _ => throw new InvalidDataException(
                $"Unknown PE optional header magic: 0x{optMagic:X4}."
            ),
        };

        // These fields are at the same offsets in both PE32 and PE32+
        // Minimum required size: PE32 needs 64 bytes (0..63), PE32+ needs 80 bytes (0..79)
        var minOptHeaderSize = isPE32Plus ? 80 : 64;
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
        var dataDirBase = optHeaderOffset + (isPE32Plus ? 112 : 96);
        if (dataDirBase + 24 > stream.Length)
        {
            throw new InvalidDataException(
                "PE optional header is too small to contain the resource data directory entry."
            );
        }

        var dataDir2FileOffset = dataDirBase + 16;

        // Section headers begin right after the optional header
        var firstSectionHeaderOffset = optHeaderOffset + sizeOfOptionalHeader;

        var info = new PEInfo
        {
            IsPE32Plus = isPE32Plus,
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
                info.ResourceSectionIndex = i;
        }

        return info;
    }

    // Walks the .rsrc directory tree yielding identifiers without loading data bytes.
    private static IEnumerable<ResourceIdentifier> ReadIdentifiers(
        BinaryReader reader,
        int sectionBase,
        int sectionSize,
        int dirOffset,
        ResourceType? type,
        ResourceName? name
    )
    {
        var absOffset = sectionBase + dirOffset;
        if (absOffset + 16 > reader.BaseStream.Length)
            yield break;

        reader.BaseStream.Position = absOffset + 12;
        var numNamed = (int)reader.ReadUInt16();
        var numId = (int)reader.ReadUInt16();
        var total = numNamed + numId;

        for (var i = 0; i < total; i++)
        {
            var entryAbs = absOffset + 16 + i * 8;
            if (entryAbs + 8 > reader.BaseStream.Length)
                yield break;

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
                    foreach (
                        var id in ReadIdentifiers(
                            reader,
                            sectionBase,
                            sectionSize,
                            subdirOffset,
                            t,
                            null
                        )
                    )
                        yield return id;
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
                    foreach (
                        var id in ReadIdentifiers(
                            reader,
                            sectionBase,
                            sectionSize,
                            subdirOffset,
                            type,
                            n
                        )
                    )
                        yield return id;
                }
                // Level 2 (language): unexpected subdirectory, skip
            }
            else
            {
                if (type is null || name is null)
                    continue;

                var langId = (int)(nameField & 0xFFFF);
                yield return new ResourceIdentifier(type, name, new Language(langId));
            }
        }
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

        // Resource string names are always encoded as UTF-16LE per the PE format spec.
        return Encoding.Unicode.GetString(reader.ReadBytes(byteCount));
    }
}
