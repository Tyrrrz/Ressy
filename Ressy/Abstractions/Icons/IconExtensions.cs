using System.IO;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Icons
{
    /// <summary>
    /// Extensions for <see cref="PortableExecutable"/> for working with icon resources.
    /// </summary>
    public static class IconExtensions
    {
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
                    if (identifier.Type.Code == ResourceType.Icon.Code ||
                        identifier.Type.Code == ResourceType.IconGroup.Code)
                    {
                        ctx.Remove(identifier);
                    }
                }
            });
        }

        /// <summary>
        /// Adds or overwrites icon and icon group resources based on the specified ICO file stream.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveIcon"/> first to remove redundant
        /// icon and icon group resources.
        /// </remarks>
        public static void SetIcon(this PortableExecutable portableExecutable, Stream iconFileStream)
        {
            var iconGroup = IconGroup.Deserialize(iconFileStream);

            portableExecutable.UpdateResources(ctx =>
            {
                // Icon resources (written as-is)
                foreach (var (icon, index) in iconGroup.Icons.Indexed())
                {
                    ctx.Set(new ResourceIdentifier(
                        ResourceType.Icon,
                        ResourceName.FromCode(index + 1)
                    ), icon.Data);
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
                    foreach (var (icon, index) in iconGroup.Icons.Indexed())
                    {
                        writer.Write(icon.Width);
                        writer.Write(icon.Height);
                        writer.Write(icon.ColorCount);
                        writer.Write((byte)0); // reserved
                        writer.Write(icon.ColorPlanes);
                        writer.Write(icon.BitsPerPixel);
                        writer.Write((uint)icon.Data.Length);
                        writer.Write((ushort)(index + 1));
                    }

                    ctx.Set(new ResourceIdentifier(
                        ResourceType.IconGroup,
                        ResourceName.FromCode(1)
                    ), buffer.ToArray());
                }
            });
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