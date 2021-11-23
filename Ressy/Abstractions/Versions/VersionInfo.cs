using System;
using System.Collections.Generic;

namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Version information associated with a portable executable file.
    /// </summary>
    // https://docs.microsoft.com/en-us/windows/win32/menurc/vs-versioninfo
    public partial class VersionInfo
    {
        /// <summary>
        /// File version.
        /// </summary>
        public Version FileVersion { get; }

        /// <summary>
        /// Product version.
        /// </summary>
        public Version ProductVersion { get; }

        /// <summary>
        /// File flags.
        /// </summary>
        public FileFlags FileFlags { get; }

        /// <summary>
        /// File's target operating system.
        /// </summary>
        public FileOperatingSystem FileOperatingSystem { get; }

        /// <summary>
        /// File type.
        /// </summary>
        public FileType FileType { get; }

        /// <summary>
        /// File sub-type.
        /// </summary>
        public FileSubType FileSubType { get; }

        /// <summary>
        /// File timestamp.
        /// </summary>
        public DateTimeOffset FileTimestamp { get; }

        /// <summary>
        /// Version attributes (contained within the StringFileInfo structure).
        /// </summary>
        public IReadOnlyDictionary<VersionAttributeName, string> Attributes { get; }

        /// <summary>
        /// File translations (contained within the VarFileInfo structure).
        /// </summary>
        public IReadOnlyList<TranslationInfo> Translations { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VersionInfo"/>.
        /// </summary>
        public VersionInfo(
            Version fileVersion,
            Version productVersion,
            FileFlags fileFlags,
            FileOperatingSystem fileOperatingSystem,
            FileType fileType,
            FileSubType fileSubType,
            DateTimeOffset fileTimestamp,
            IReadOnlyDictionary<VersionAttributeName, string> attributes,
            IReadOnlyList<TranslationInfo> translations)
        {
            FileVersion = fileVersion;
            ProductVersion = productVersion;
            FileFlags = fileFlags;
            FileOperatingSystem = fileOperatingSystem;
            FileType = fileType;
            FileSubType = fileSubType;
            FileTimestamp = fileTimestamp;
            Attributes = attributes;
            Translations = translations;
        }
    }
}