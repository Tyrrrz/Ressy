using System.IO;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Icons
{
    /// <summary>
    /// Extensions for <see cref="PortableExecutable"/> for working with icon resources.
    /// </summary>
    public static class IconExtensions
    {
        private static void SetIconGroupResource(
            this PortableExecutable portableExecutable,
            IconFile iconFile, ResourceName name)
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer);

            // Copy header data
            writer.Write(iconFile.GetHeaderData());

            // Copy reformatted icon metadata
            foreach (var (icon, index) in iconFile.GetIcons().Indexed())
            {
                // Write metadata except for the offset (skip last 4 bytes)
                var metaData = icon.GetMetaData();
                writer.Write(metaData, 0, metaData.Length - 4);

                // Write icon index (instead of offset)
                writer.Write((short)(index + 1));
            }

            portableExecutable.SetResource(
                new ResourceIdentifier(ResourceType.FromCode(StandardResourceTypeCode.GroupIcon), name),
                buffer.ToArray()
            );
        }

        private static void SetIconResource(
            this PortableExecutable portableExecutable,
            IconFileEntry iconFileEntry, ResourceName name) =>
            portableExecutable.SetResource(
                new ResourceIdentifier(ResourceType.FromCode(StandardResourceTypeCode.Icon), name),
                iconFileEntry.GetIconData()
            );

        /// <summary>
        /// Removes all existing icon and icon group resources.
        /// </summary>
        public static void RemoveIcon(this PortableExecutable portableExecutable)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var identifier in identifiers)
                {
                    if (identifier.Type.Code is
                        (int)StandardResourceTypeCode.Icon or
                        (int)StandardResourceTypeCode.GroupIcon)
                    {
                        ctx.Remove(identifier);
                    }
                }
            });
        }

        /// <summary>
        /// Adds or overwrites icon and icon group resources based on the specified ICO file stream.
        /// Input stream must support seeking.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveIcon"/> first to remove redundant
        /// icon and icon group resources.
        /// </remarks>
        public static void SetIcon(this PortableExecutable portableExecutable, Stream iconStream)
        {
            using var iconFile = IconFile.Open(iconStream);

            portableExecutable.SetIconGroupResource(iconFile, ResourceName.FromCode(1));

            foreach (var (icon, index) in iconFile.GetIcons().Indexed())
                portableExecutable.SetIconResource(icon, ResourceName.FromCode(index + 1));
        }

        /// <summary>
        /// Adds or overwrites icon and icon group resources based on the specified ICO file.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveIcon"/> first to remove redundant
        /// icon and icon group resources.
        /// </remarks>
        public static void SetIcon(this PortableExecutable portableExecutable, string iconFilePath)
        {
            using var iconStream = File.OpenRead(iconFilePath);
            portableExecutable.SetIcon(iconStream);
        }
    }
}