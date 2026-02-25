using System;
using System.Collections.Generic;
using System.Linq;
using Ressy.Utils.Extensions;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with string table resources.
/// </summary>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public static class StringTableExtensions
{
    private static ResourceIdentifier? TryGetStringTableBlockResourceIdentifier(
        PortableExecutable portableExecutable,
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

    private static Resource? TryGetStringTableBlockResource(
        PortableExecutable portableExecutable,
        int blockId,
        Language language
    )
    {
        var identifier = TryGetStringTableBlockResourceIdentifier(
            portableExecutable,
            blockId,
            language
        );

        return identifier is not null ? portableExecutable.TryGetResource(identifier) : null;
    }

    /// <inheritdoc cref="StringTableExtensions" />
    extension(Resource resource)
    {
        /// <summary>
        /// Reads the specified resource as a string table resource and
        /// deserializes its data to the corresponding structural representation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the resource name is not an ordinal.
        /// </exception>
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
        /// <summary>
        /// Gets all string table resource blocks and returns a unified view over all of them.
        /// Retrieves resource blocks in the specified language or in the neutral language if no language is specified.
        /// Returns <c>null</c> if no string table resources exist.
        /// </summary>
        public StringTable? TryGetStringTable(Language? language = null)
        {
            var targetLanguage = language ?? Language.NeutralDefault;

            var blockIdentifiers = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code && r.Language.Id == targetLanguage.Id
                );

            var blocks = new Dictionary<int, byte[]>();

            foreach (var identifier in blockIdentifiers)
            {
                if (identifier.Name.Code is not { } blockId)
                    continue;

                var resource = portableExecutable.TryGetResource(identifier);
                if (resource is null)
                    continue;

                blocks[blockId] = resource.Data;
            }

            return blocks.Count > 0 ? StringTable.Deserialize(blocks) : null;
        }

        /// <summary>
        /// Gets all string table resource blocks and returns a unified view over all of them.
        /// Retrieves resource blocks in the specified language or in the neutral language if no language is specified.
        /// </summary>
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
        /// Adds or overwrites string table resource blocks with the specified data.
        /// Removes blocks that existed in the previous string table but not in the new one.
        /// </summary>
        public void SetStringTable(StringTable stringTable, Language? language = null)
        {
            var targetLanguage = language ?? Language.NeutralDefault;

            // Find block IDs that currently exist for this language
            var existingBlockIds = portableExecutable
                .GetResourceIdentifiers()
                .Where(r =>
                    r.Type.Code == ResourceType.String.Code && r.Language.Id == targetLanguage.Id
                )
                .Select(r => r.Name.Code)
                .WhereNotNull()
                .ToHashSet();

            var blocks = stringTable.Serialize();
            var newBlockIds = blocks.Select((_, i) => i + 1).ToHashSet();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var (i, block) in blocks.Index())
                {
                    ctx.Set(
                        new ResourceIdentifier(
                            ResourceType.String,
                            ResourceName.FromCode(i + 1),
                            targetLanguage
                        ),
                        block
                    );
                }

                // Remove blocks that existed in the previous string table but aren't in the new one
                foreach (var staleBlockId in existingBlockIds)
                {
                    if (newBlockIds.Contains(staleBlockId))
                        continue;

                    ctx.Remove(
                        new ResourceIdentifier(
                            ResourceType.String,
                            ResourceName.FromCode(staleBlockId),
                            targetLanguage
                        )
                    );
                }
            });
        }

        /// <summary>
        /// Modifies the currently stored string table resource blocks.
        /// If there are no existing string table resources, an empty one will be created.
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
