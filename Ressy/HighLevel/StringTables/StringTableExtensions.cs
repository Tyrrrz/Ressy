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
        /// Sets the string table resources to the specified string table.
        /// Blocks that previously existed but are not present in the new string table are removed.
        /// </summary>
        public void SetStringTable(StringTable stringTable, Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;

            // Find block IDs that currently exist for this language
            var existingBlockIds = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code
                    && r.Language.Id == targetLanguage.Id
                    && r.Name.Code is not null
                )
                .Select(r => r.Name.Code!.Value)
                .ToHashSet();

            var blocks = stringTable
                .Strings.GroupBy(kv => StringTable.GetBlockId(kv.Key))
                .ToArray();
            var newBlockIds = blocks.Select(b => b.Key).ToHashSet();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var block in blocks)
                {
                    // The block must always contain exactly BlockSize entries, with string.Empty
                    // representing absent string IDs.
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

                // Remove blocks that existed in the previous string table but aren't in the new one
                foreach (var staleBlockId in existingBlockIds)
                {
                    if (!newBlockIds.Contains(staleBlockId))
                    {
                        ctx.Remove(
                            new ResourceIdentifier(
                                ResourceType.String,
                                ResourceName.FromCode(staleBlockId),
                                targetLanguage
                            )
                        );
                    }
                }
            });
        }

        /// <summary>
        /// Modifies the currently stored string table resource blocks.
        /// </summary>
        public void SetStringTable(Action<StringTableBuilder> modify, Language? language = null)
        {
            var builder = new StringTableBuilder();

            var existing = portableExecutable.TryGetStringTable(language);
            if (existing is not null)
                builder.SetAll(existing);

            modify(builder);

            portableExecutable.SetStringTable(builder.Build(), language);
        }
    }
}
