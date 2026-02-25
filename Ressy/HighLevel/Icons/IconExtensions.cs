using System.IO;
using System.Linq;

namespace Ressy.HighLevel.Icons;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with icon resources.
/// </summary>
public static class IconExtensions
{
    /// <inheritdoc cref="IconExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        /// <summary>
        /// Removes all existing icon and icon group resources.
        /// </summary>
        public void RemoveIcon()
        {
            var resources = portableExecutable
                .GetResources()
                .Where(kv =>
                    kv.Key.Type.Code != ResourceType.Icon.Code
                    && kv.Key.Type.Code != ResourceType.IconGroup.Code
                )
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            portableExecutable.SetResources(resources);
        }

        /// <summary>
        /// Adds or overwrites icon and icon group resources based on the specified ICO file stream.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveIcon" /> first to remove redundant
        /// icon and icon group resources left from previously existing icons.
        /// </remarks>
        public void SetIcon(Stream iconFileStream)
        {
            var iconGroup = IconGroup.Deserialize(iconFileStream);

            var resources = portableExecutable
                .GetResources()
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Icon resources (written as-is)
            foreach (var (i, icon) in iconGroup.Icons.Index())
            {
                resources[new ResourceIdentifier(ResourceType.Icon, ResourceName.FromCode(i + 1))] =
                    icon.Data;
            }

            // Icon group resource (offset is replaced with icon index)
            {
                using var buffer = new MemoryStream();
                using var writer = new BinaryWriter(buffer);

                // Header
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)iconGroup.Icons.Count);

                // Icon directory
                foreach (var (i, icon) in iconGroup.Icons.Index())
                {
                    writer.Write(icon.Width);
                    writer.Write(icon.Height);
                    writer.Write(icon.ColorCount);
                    writer.Write((byte)0); // reserved
                    writer.Write(icon.ColorPlanes);
                    writer.Write(icon.BitsPerPixel);
                    writer.Write((uint)icon.Data.Length);
                    writer.Write((ushort)(i + 1));
                }

                resources[
                    new ResourceIdentifier(ResourceType.IconGroup, ResourceName.FromCode(1))
                ] = buffer.ToArray();
            }

            portableExecutable.SetResources(resources);
        }

        /// <summary>
        /// Adds or overwrites icon and icon group resources based on the specified ICO file.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveIcon" /> first to remove redundant
        /// icon and icon group resources left from previously existing icons.
        /// </remarks>
        public void SetIcon(string iconFilePath)
        {
            using var iconStream = File.OpenRead(iconFilePath);
            portableExecutable.SetIcon(iconStream);
        }
    }
}
