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
        /// Gets all string table resource blocks and returns a unified view over all of them.
        /// Returns <c>null</c> if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is specified, retrieves only blocks in that language.
        /// If <paramref name="language" /> is not specified, retrieves blocks in the neutral language.
        /// </remarks>
        public StringTable? TryGetStringTable(Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;

            var blockIdentifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code && r.Language.Id == targetLanguage.Id
                );

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
                    if (!string.IsNullOrEmpty(blockStrings[i]))
                        strings[baseStringId + i] = blockStrings[i];
                }
            }

            return strings.Any() ? new StringTable(strings) : null;
        }

        /// <summary>
        /// Gets all string table resource blocks and returns a unified view over all of them.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is specified, retrieves only blocks in that language.
        /// If <paramref name="language" /> is not specified, retrieves blocks in the neutral language.
        /// </remarks>
        public StringTable GetStringTable(Language? language = null) =>
            portableExecutable.TryGetStringTable(language)
            ?? throw new InvalidOperationException("String table resource does not exist.");

        /// <summary>
        /// Gets the string with the specified ID from the string table resources.
        /// Returns <c>null</c> if the string doesn't exist.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method looks for the string
        /// in the neutral language.
        /// </remarks>
        public string? TryGetString(int stringId, Language? language = null)
        {
            if (stringId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stringId),
                    "String ID must be non-negative."
                );
            }

            var blockId = StringTable.GetBlockId(stringId);
            var blockIndex = StringTable.GetBlockIndex(stringId);

            var targetLanguage = language ?? Language.Neutral;

            var identifier = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code
                    && r.Name.Code == blockId
                    && r.Language.Id == targetLanguage.Id
                )
                .FirstOrDefault();

            if (identifier is null)
                return null;

            var resource = portableExecutable.TryGetResource(identifier);
            if (resource is null)
                return null;

            var strings = StringTable.Deserialize(resource.Data);
            var value = strings[blockIndex];

            return !string.IsNullOrEmpty(value) ? value : null;
        }

        /// <summary>
        /// Gets the string with the specified ID from the string table resources.
        /// </summary>
        /// <remarks>
        /// If <paramref name="language" /> is not specified, this method looks for the string
        /// in the neutral language.
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

            var blocks = stringTable.Strings.GroupBy(kv => StringTable.GetBlockId(kv.Key));

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var block in blocks)
                {
                    // Use Enumerable.Repeat(string.Empty, BlockSize) rather than new string[BlockSize]:
                    // (1) new string[BlockSize] produces null elements, which Serialize does not accept.
                    // (2) The block must always contain exactly BlockSize entries, with string.Empty
                    //     representing absent string IDs.
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
        /// Modifies string table resources using the specified builder configuration.
        /// If string table resources already exist, uses them as the base.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveStringTable" /> first to remove redundant
        /// string table resources.
        /// </remarks>
        public void SetStringTable(Action<StringTableBuilder> modify, Language? language = null)
        {
            var builder = new StringTableBuilder();

            var existing = portableExecutable.TryGetStringTable(language);
            if (existing is not null)
                builder.SetAll(existing);

            modify(builder);

            portableExecutable.SetStringTable(builder.Build(), language);
        }

        /// <summary>
        /// Adds or overwrites a string in the string table resource with the specified ID and value.
        /// </summary>
        public void SetString(int stringId, string value, Language? language = null)
        {
            if (stringId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stringId),
                    "String ID must be non-negative."
                );
            }

            portableExecutable.SetStringTable(b => b.SetString(stringId, value), language);
        }
    }
}
