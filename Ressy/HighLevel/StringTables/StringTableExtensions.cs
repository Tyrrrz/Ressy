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
    extension(PortableExecutable portableExecutable)
    {
        /// <summary>
        /// Gets all strings from the string table resources.
        /// Returns <c>null</c> if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method retrieves strings from
        /// all available string table resources, giving preference to resources in the neutral language
        /// when multiple languages contain the same string ID.
        /// </remarks>
        public StringTable? TryGetStringTable(Language? language = null)
        {
            var blockIdentifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code)
                .Where(r => language is null || r.Language.Id == language.Value.Id)
                .GroupBy(r => r.Name.Code)
                // No-op when a specific language is requested (all entries have the same language),
                // but prefers neutral language when no language is specified.
                .Select(g => g.OrderBy(r => r.Language.Id == Language.Neutral.Id ? 0 : 1).First());

            var strings = new Dictionary<int, string>();

            foreach (var identifier in blockIdentifiers)
            {
                if (identifier.Name.Code is null)
                    continue;

                var resource = portableExecutable.TryGetResource(identifier);
                if (resource is null)
                    continue;

                var baseStringId = (identifier.Name.Code.Value - 1) * StringTable.BlockSize;
                var blockStrings = StringTable.Deserialize(resource.Data);

                for (var i = 0; i < StringTable.BlockSize; i++)
                {
                    if (blockStrings[i].Length > 0)
                        strings[baseStringId + i] = blockStrings[i];
                }
            }

            return strings.Any() ? new StringTable(strings) : null;
        }

        /// <summary>
        /// Gets all strings from the string table resources.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method retrieves strings from
        /// all available string table resources, giving preference to resources in the neutral language
        /// when multiple languages contain the same string ID.
        /// </remarks>
        public StringTable GetStringTable(Language? language = null) =>
            portableExecutable.TryGetStringTable(language)
            ?? throw new InvalidOperationException("String table resource does not exist.");

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

            var candidates = portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.String.Code && r.Name.Code == blockId)
                .Where(r => language is null || r.Language.Id == language.Value.Id);

            if (language is null)
                candidates = candidates.OrderBy(r => r.Language.Id != Language.Neutral.Id);

            var identifier = candidates.FirstOrDefault();
            if (identifier is null)
                return null;

            var resource = portableExecutable.TryGetResource(identifier);
            if (resource is null)
                return null;

            var strings = StringTable.Deserialize(resource.Data);
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
        /// <remarks>
        /// Consider calling <see cref="RemoveStringTable" /> first to remove redundant
        /// string table resources.
        /// </remarks>
        public void SetStringTable(StringTable stringTable, Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;

            var blocks = stringTable.GroupBy(kv => StringTable.GetBlockId(kv.Key));

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

            var strings =
                portableExecutable
                    .TryGetStringTable(targetLanguage)
                    ?.ToDictionary(kv => kv.Key, kv => kv.Value)
                ?? new Dictionary<int, string>();

            strings[stringId] = value;

            portableExecutable.SetStringTable(new StringTable(strings), targetLanguage);
        }
    }
}
