using System;
using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with string table resources.
/// </summary>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public static class StringTableExtensions
{
    /// <inheritdoc cref="StringTableExtensions" />
    extension(Resource resource)
    {
        /// <summary>
        /// Reads the specified resource as a string table block and returns the 16 strings it contains.
        /// </summary>
        public string[] ReadAsStringTable() => StringTable.Deserialize(resource.Data);
    }

    /// <inheritdoc cref="StringTableExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        private ResourceIdentifier? TryGetStringTableResourceIdentifier(
            Language? language = null
        ) =>
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code)
                .Where(r => language is null || r.Language.Id == language.Value.Id)
                .OrderBy(r => r.Language.Id == Language.Neutral.Id ? 0 : 1)
                .ThenBy(r => r.Name.Code ?? int.MaxValue)
                .FirstOrDefault();

        private Resource? TryGetStringTableResource(Language? language = null)
        {
            var identifier = portableExecutable.TryGetStringTableResourceIdentifier(language);
            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier);
        }

        /// <summary>
        /// Gets all strings from the string table resources as a dictionary mapping string IDs to values.
        /// Returns <c>null</c> if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method retrieves strings from
        /// all available string table resources, giving preference to resources in the neutral language
        /// when multiple languages contain the same string ID.
        /// </remarks>
        public IReadOnlyDictionary<int, string>? TryGetStringTable(Language? language = null)
        {
            if (portableExecutable.TryGetStringTableResourceIdentifier(language) is null)
                return null;

            var neutralLanguageId = Language.Neutral.Id;
            var blockIdentifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code && r.Name.Code is not null)
                .Where(r => language is null || r.Language.Id == language.Value.Id)
                .GroupBy(r => r.Name.Code!.Value)
                .Select(g =>
                    language is null
                        ? g.OrderBy(r => r.Language.Id == neutralLanguageId ? 0 : 1).First()
                        : g.First()
                );

            var result = new Dictionary<int, string>();

            foreach (var identifier in blockIdentifiers)
            {
                var resource = portableExecutable.TryGetResource(identifier);
                if (resource is null)
                    continue;

                var blockId = identifier.Name.Code!.Value;
                var baseStringId = (blockId - 1) * StringTable.BlockSize;
                var strings = resource.ReadAsStringTable();

                for (var i = 0; i < StringTable.BlockSize; i++)
                {
                    if (strings[i].Length > 0)
                        result[baseStringId + i] = strings[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all strings from the string table resources as a dictionary mapping string IDs to values.
        /// Returns an empty dictionary if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method retrieves strings from
        /// all available string table resources, giving preference to resources in the neutral language
        /// when multiple languages contain the same string ID.
        /// </remarks>
        public IReadOnlyDictionary<int, string> GetStringTable(Language? language = null) =>
            portableExecutable.TryGetStringTable(language) ?? new Dictionary<int, string>();

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
            if (stringId < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(stringId),
                    "String ID must be non-negative."
                );

            var blockId = StringTable.GetBlockId(stringId);
            var blockIndex = StringTable.GetBlockIndex(stringId);

            var neutralLanguageId = Language.Neutral.Id;
            var candidates = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code && r.Name.Code == blockId)
                .Where(r => language is null || r.Language.Id == language.Value.Id);

            if (language is null)
                candidates = candidates.OrderBy(r => r.Language.Id == neutralLanguageId ? 0 : 1);

            var identifier = candidates.FirstOrDefault();
            if (identifier is null)
                return null;

            var resource = portableExecutable.TryGetResource(identifier);
            if (resource is null)
                return null;

            var strings = resource.ReadAsStringTable();
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
        /// Adds or overwrites all strings in the string table resources with the specified mapping
        /// of string IDs to values.
        /// </summary>
        public void SetStringTable(
            IReadOnlyDictionary<int, string> strings,
            Language? language = null
        )
        {
            var targetLanguage = language ?? Language.Neutral;

            var blocks = strings.GroupBy(kv => StringTable.GetBlockId(kv.Key));

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var block in blocks)
                {
                    var blockData = Enumerable
                        .Repeat(string.Empty, StringTable.BlockSize)
                        .ToArray();

                    foreach (var kv in block)
                        blockData[StringTable.GetBlockIndex(kv.Key)] = kv.Value;

                    ctx.Set(
                        new ResourceIdentifier(
                            ResourceType.String,
                            ResourceName.FromCode(block.Key),
                            targetLanguage
                        ),
                        StringTable.Serialize(blockData)
                    );
                }
            });
        }

        /// <summary>
        /// Adds or overwrites a string in the string table resource with the specified ID and value.
        /// </summary>
        public void SetString(int stringId, string value, Language? language = null)
        {
            if (stringId < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(stringId),
                    "String ID must be non-negative."
                );

            var targetLanguage = language ?? Language.Neutral;
            var blockId = StringTable.GetBlockId(stringId);
            var blockIndex = StringTable.GetBlockIndex(stringId);

            var identifier = new ResourceIdentifier(
                ResourceType.String,
                ResourceName.FromCode(blockId),
                targetLanguage
            );

            // Load existing block data or start with empty entries
            var strings = Enumerable.Repeat(string.Empty, StringTable.BlockSize).ToArray();

            var existingResource = portableExecutable.TryGetResource(identifier);
            if (existingResource is not null)
            {
                var existingStrings = existingResource.ReadAsStringTable();
                Array.Copy(existingStrings, strings, StringTable.BlockSize);
            }

            strings[blockIndex] = value;

            portableExecutable.SetResource(identifier, StringTable.Serialize(strings));
        }
    }
}
