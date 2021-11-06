using System.IO;
using Ressy.Identification;
using Ressy.Utils;

namespace Ressy
{
    public partial class PortableExecutable
    {
        private static void SetIconGroupResource(
            string imageFilePath,
            IconFile iconFile,
            ResourceLanguage language)
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer);

            // Copy header data
            writer.Write(iconFile.GetHeaderData());

            // Copy individual icon metadata
            var index = (short)1;
            foreach (var icon in iconFile.GetIcons())
            {
                // Write metadata except for the last 4 bytes (no offset)
                var metaData = icon.GetMetaData();
                writer.Write(metaData, 0, metaData.Length - 4);

                // Write icon index
                writer.Write(index++);
            }

            SetResource(
                imageFilePath,
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.GroupIcon),
                    ResourceName.FromCode(1),
                    language
                ),
                buffer.ToArray()
            );
        }

        private static void SetIconResource(
            string imageFilePath,
            IconFileEntry iconFileEntry,
            ResourceName name,
            ResourceLanguage language)
        {
            SetResource(
                imageFilePath,
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Icon),
                    name,
                    language
                ),
                iconFileEntry.GetIconData()
            );
        }

        /// <summary>
        /// Removes existing icon resources.
        /// </summary>
        public static void RemoveIcon(string imageFilePath, ResourceLanguage language)
        {
            var resources = GetResources(imageFilePath);
            using var context = ResourceUpdateContext.Create(imageFilePath);

            foreach (var resource in resources)
            {
                // Skip other languages
                if (resource.Language.Id != language.Id)
                    continue;

                if (resource.Type.Code is (int)StandardResourceTypeCode.Icon or (int)StandardResourceTypeCode.GroupIcon)
                    context.Remove(resource);
            }
        }

        /// <summary>
        /// Removes existing icon resources.
        /// </summary>
        public static void RemoveIcon(string imageFilePath) =>
            RemoveIcon(imageFilePath, ResourceLanguage.Neutral);

        /// <summary>
        /// Removes existing icon resources and adds icon data from the specified ICO file.
        /// </summary>
        public static void SetIcon(string imageFilePath, string iconFilePath, ResourceLanguage language)
        {
            // Read icon file
            using var iconFile = IconFile.Open(iconFilePath);

            // Remove existing icon resources from image
            RemoveIcon(imageFilePath, language);

            // Add icon group resource
            SetIconGroupResource(imageFilePath, iconFile, language);

            // Add icon resources
            var index = 1;
            foreach (var icon in iconFile.GetIcons())
                SetIconResource(imageFilePath, icon, ResourceName.FromCode(index++), language);
        }

        /// <summary>
        /// Clears existing icon resources and adds icon data from the specified ICO file.
        /// </summary>
        public static void SetIcon(string imageFilePath, string iconFilePath) =>
            SetIcon(imageFilePath, iconFilePath, ResourceLanguage.Neutral);
    }
}