using System;
using System.Collections.Generic;
using System.Linq;
using Ressy.Utils.Extensions;

namespace Ressy.Strings;

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
        /// Reads the specified resource as a string table resource and
        /// deserializes its data to the corresponding structural representation.
        /// </summary>
        public StringTableBlock ReadAsStringTableBlock()
        {
            var blockId =
                resource.Identifier.Name.Code
                ?? throw new InvalidOperationException(
                    "Cannot read resource as a string table block: "
                        + "the resource name is not an ordinal."
                );

            return StringTableBlock.Deserialize(blockId, resource.Data);
        }
    }

    /// <inheritdoc cref="StringTableExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        private ResourceIdentifier? TryGetStringTableBlockResourceIdentifier(
            int blockId,
            Language language
        ) =>
            portableExecutable
                .GetResourceIdentifiers()
                .FirstOrDefault(r =>
                    r.Type.Code == ResourceType.String.Code
                    && r.Name.Code == blockId
                    && r.Language.Id == language.Id
                );

        private Resource? TryGetStringTableBlockResource(int blockId, Language language)
        {
            var identifier = portableExecutable.TryGetStringTableBlockResourceIdentifier(
                blockId,
                language
            );

            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier);
        }

        /// <summary>
        /// Gets all string table resource blocks, deserializes them, and returns a unified view over them.
        /// Returns <c>null</c> if no string table resources exist.
        /// </summary>
        /// <remarks>
        /// If the language is specified, this method retrieves string table resources only in that language.
        /// If the language is not specified, this method retrieves string table resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public StringTable? TryGetStringTable(Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;

            var blockIdentifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code && r.Language.Id == targetLanguage.Id
                );

            var blocks = new List<StringTableBlock>();

            foreach (var identifier in blockIdentifiers)
            {
                if (identifier.Name.Code is not { } blockId)
                    continue;

                var block = portableExecutable
                    .TryGetStringTableBlockResource(blockId, targetLanguage)
                    ?.ReadAsStringTableBlock();

                if (block is null)
                    continue;

                blocks.Add(block);
            }

            return blocks.Count > 0 ? StringTable.FromBlocks(blocks) : null;
        }

        /// <summary>
        /// Gets all string table resource blocks, deserializes them, and returns a unified view over them.
        /// </summary>
        /// <remarks>
        /// If the language is specified, this method retrieves string table resources only in that language.
        /// If the language is not specified, this method retrieves string table resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public StringTable GetStringTable(Language? language = null) =>
            portableExecutable.TryGetStringTable(language)
            ?? throw new InvalidOperationException("String table resource does not exist.");

        /// <summary>
        /// Removes all existing string table resources.
        /// </summary>
        public void RemoveStringTable() =>
            portableExecutable.RemoveResources(r => r.Type.Code == ResourceType.String.Code);

        /// <summary>
        /// Adds or overwrites string table resource blocks with the specified data.
        /// </summary>
        /// <remarks>
        /// If there are existing string table resource blocks (based on <see cref="TryGetStringTable" /> rules),
        /// they will be automatically removed.
        /// </remarks>
        public void SetStringTable(StringTable stringTable, Language? language = null)
        {
            var targetLanguage = language ?? Language.Neutral;

            // Find block IDs that currently exist for this language
            var existingBlockIds = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code && r.Language.Id == targetLanguage.Id
                )
                .Select(r => r.Name.Code)
                .WhereNotNull()
                .ToHashSet();

            var blocks = stringTable.ToBlocks();
            var newBlockIds = blocks.Select(b => b.BlockId).ToHashSet();

            var resources = portableExecutable
                .GetResources()
                .ToDictionary(r => r.Identifier, r => r.Data);

            foreach (var block in blocks)
            {
                resources[
                    new ResourceIdentifier(
                        ResourceType.String,
                        ResourceName.FromCode(block.BlockId),
                        targetLanguage
                    )
                ] = block.Serialize();
            }

            // Remove blocks that existed in the previous string table but aren't in the new one
            foreach (var staleBlockId in existingBlockIds)
            {
                if (newBlockIds.Contains(staleBlockId))
                    continue;

                resources.Remove(
                    new ResourceIdentifier(
                        ResourceType.String,
                        ResourceName.FromCode(staleBlockId),
                        targetLanguage
                    )
                );
            }

            portableExecutable.SetResources(
                resources.Select(kv => new Resource(kv.Key, kv.Value)).ToArray(),
                true
            );
        }

        /// <summary>
        /// Modifies the currently stored string table resource blocks.
        /// </summary>
        /// <remarks>
        /// If there are existing string table resource blocks (based on <see cref="TryGetStringTable" /> rules),
        /// they will be used as the base for modification.
        /// If there are no existing string table resource blocks, they will be created as necessary.
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
    }
}
