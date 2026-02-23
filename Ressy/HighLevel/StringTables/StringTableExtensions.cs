using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with string table resources.
/// </summary>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public static class StringTableExtensions
{
    private const int BlockSize = 16;

    private static int GetBlockId(int stringId) => (stringId >> 4) + 1;

    private static int GetBlockIndex(int stringId) => stringId & 0x0F;

    private static string[] ReadBlock(byte[] data)
    {
        var strings = new string[BlockSize];

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.Unicode);

        for (var i = 0; i < BlockSize; i++)
        {
            var length = reader.ReadUInt16();
            strings[i] = new string(reader.ReadChars(length));
        }

        return strings;
    }

    private static byte[] WriteBlock(string[] strings)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.Unicode);

        for (var i = 0; i < BlockSize; i++)
        {
            var str = strings[i];
            writer.Write((ushort)str.Length);
            foreach (var c in str)
                writer.Write(c);
        }

        return stream.ToArray();
    }

    /// <inheritdoc cref="StringTableExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        /// <summary>
        /// Gets all strings from the string table resources as a dictionary mapping string IDs to values.
        /// Returns an empty dictionary if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method retrieves strings from
        /// all available string table resources, giving preference to resources in the neutral language
        /// when multiple languages contain the same string ID.
        /// </remarks>
        public IReadOnlyDictionary<int, string> GetStringTable(Language? language = null)
        {
            var identifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code)
                .Where(r => language is null || r.Language.Id == language.Value.Id);

            // Group by block ID and prefer neutral language entries
            var neutralLanguageId = Language.Neutral.Id;
            var blockGroups = identifiers
                .Where(r => r.Name.Code is not null)
                .GroupBy(r => r.Name.Code!.Value)
                .Select(g => g.OrderBy(r => r.Language.Id == neutralLanguageId ? 0 : 1).First());

            var result = new Dictionary<int, string>();

            foreach (var identifier in blockGroups)
            {
                var resource = portableExecutable.TryGetResource(identifier);
                if (resource is null)
                    continue;

                var blockId = identifier.Name.Code!.Value;
                var baseStringId = (blockId - 1) * BlockSize;
                var strings = ReadBlock(resource.Data);

                for (var i = 0; i < BlockSize; i++)
                {
                    if (strings[i].Length > 0)
                        result[baseStringId + i] = strings[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the string with the specified ID from the string table resources.
        /// Returns <c>null</c> if the string doesn't exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method gives preference
        /// to resources in the neutral language.
        /// </remarks>
        public string? TryGetString(int stringId, Language? language = null)
        {
            var blockId = GetBlockId(stringId);
            var blockIndex = GetBlockIndex(stringId);

            var neutralLanguageId = Language.Neutral.Id;
            var identifier = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code && r.Name.Code == blockId)
                .Where(r => language is null || r.Language.Id == language.Value.Id)
                .OrderBy(r => r.Language.Id == neutralLanguageId ? 0 : 1)
                .FirstOrDefault();

            if (identifier is null)
                return null;

            var resource = portableExecutable.TryGetResource(identifier);
            if (resource is null)
                return null;

            var strings = ReadBlock(resource.Data);
            var value = strings[blockIndex];

            return value.Length > 0 ? value : null;
        }

        /// <summary>
        /// Gets the string with the specified ID from the string table resources.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method gives preference
        /// to resources in the neutral language.
        /// </remarks>
        public string GetString(int stringId, Language? language = null) =>
            portableExecutable.TryGetString(stringId, language)
            ?? throw new InvalidOperationException(
                $"String with ID '{stringId}' does not exist in the string table."
            );

        /// <summary>
        /// Removes all existing string table resources.
        /// </summary>
        public void RemoveStringTable()
        {
            var identifiers = portableExecutable.GetResourceIdentifiers();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var identifier in identifiers)
                {
                    if (identifier.Type.Code == ResourceType.String.Code)
                        ctx.Remove(identifier);
                }
            });
        }

        /// <summary>
        /// Adds or overwrites a string in the string table resource with the specified ID and value.
        /// </summary>
        public void SetString(int stringId, string value, Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;
            var blockId = GetBlockId(stringId);
            var blockIndex = GetBlockIndex(stringId);

            var identifier = new ResourceIdentifier(
                ResourceType.String,
                ResourceName.FromCode(blockId),
                targetLanguage
            );

            // Load existing block data or start with empty entries
            var strings = Enumerable.Repeat(string.Empty, BlockSize).ToArray();

            var existingResource = portableExecutable.TryGetResource(identifier);
            if (existingResource is not null)
            {
                var existingStrings = ReadBlock(existingResource.Data);
                Array.Copy(existingStrings, strings, BlockSize);
            }

            strings[blockIndex] = value;

            portableExecutable.SetResource(identifier, WriteBlock(strings));
        }
    }
}
