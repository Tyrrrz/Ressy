using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Extensions for <see cref="PortableExecutable"/> for working with version resources.
    /// </summary>
    public static class VersionExtensions
    {
        private static Resource? TryGetVersionInfoResource(this PortableExecutable portableExecutable)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers()
                .Where(r => r.Type.Code == (int)StandardResourceTypeCode.Version)
                .ToArray();

            var identifier =
                // Among neutral language resources, find one with the lowest ordinal name (ID)
                identifiers
                    .Where(r => r.Language.Id == ResourceLanguage.Neutral.Id)
                    .Where(r => r.Name.Code is not null)
                    .OrderBy(r => r.Name.Code)
                    .FirstOrDefault() ??
                // If there are no such resources, pick whichever
                identifiers.FirstOrDefault();

            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier);
        }

        public static VersionInfo? TryGetVersionInfo(this PortableExecutable portableExecutable)
        {
            var data = portableExecutable.TryGetVersionInfoResource()?.Data;
            if (data is null)
                return null;

            return VersionInfo.Deserialize(data);
        }

        public static VersionInfo GetVersionInfo(this PortableExecutable portableExecutable) =>
            portableExecutable.TryGetVersionInfo() ??
            throw new InvalidOperationException("Version info resource does not exist.");

        private static FileType GetFileType(this PortableExecutable portableExecutable) =>
            Path.GetExtension(portableExecutable.FilePath).ToLowerInvariant() switch
            {
                ".dll" => FileType.Dll,
                ".exe" => FileType.App,
                _ => FileType.Unknown
            };

        private static DateTimeOffset GetFileTimestamp(this PortableExecutable portableExecutable) =>
            new(new FileInfo(portableExecutable.FilePath).CreationTimeUtc, TimeSpan.Zero);

        private static VersionInfo CreateVersionInfo(this PortableExecutable portableExecutable) => new(
            new Version(1, 0, 0, 0),
            new Version(1, 0, 0, 0),
            FileFlags.None,
            FileOperatingSystem.Windows32 | FileOperatingSystem.NT,
            portableExecutable.GetFileType(),
            FileSubType.Unknown,
            portableExecutable.GetFileTimestamp(),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new[] { new TranslationInfo(0, Encoding.Unicode.CodePage) }
        );

        /// <summary>
        /// Removes all existing version info resources.
        /// </summary>
        public static void RemoveVersionInfo(this PortableExecutable portableExecutable)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var identifier in identifiers)
                {
                    if (identifier.Type.Code is (int)StandardResourceTypeCode.Version)
                        ctx.Remove(identifier);
                }
            });
        }

        public static void SetVersionInfo(this PortableExecutable portableExecutable, VersionInfo versionInfo) =>
            portableExecutable.SetResource(new ResourceIdentifier(
                ResourceType.FromCode(StandardResourceTypeCode.Version),
                ResourceName.FromCode(1)
            ), versionInfo.Serialize());
    }
}