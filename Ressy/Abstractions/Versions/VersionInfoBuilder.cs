using System;
using System.Collections.Generic;

namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Builder for <see cref="VersionInfo"/>.
    /// </summary>
    public class VersionInfoBuilder
    {
        private Version _fileVersion = new(1, 0, 0, 0);
        private Version _productVersion = new(1, 0, 0, 0);
        private FileFlags _fileFlags = FileFlags.None;
        private FileOperatingSystem _fileOperatingSystem = FileOperatingSystem.Windows32;
        private FileType _fileType = FileType.App;
        private FileSubType _fileSubType = FileSubType.Unknown;
        private DateTimeOffset _fileTimestamp = DateTimeOffset.Now;
        private readonly Dictionary<VersionAttributeName, string> _attributes = new();
        private readonly List<TranslationInfo> _translations = new();

        /// <summary>
        /// Sets file version.
        /// </summary>
        public VersionInfoBuilder SetFileVersion(Version fileVersion)
        {
            _fileVersion = fileVersion;
            _attributes[VersionAttributeName.FileVersion] = fileVersion.ToString(4);
            return this;
        }

        /// <summary>
        /// Sets product version.
        /// </summary>
        public VersionInfoBuilder SetProductVersion(Version productVersion)
        {
            _productVersion = productVersion;
            _attributes[VersionAttributeName.ProductVersion] = productVersion.ToString(4);
            return this;
        }

        /// <summary>
        /// Sets file flags.
        /// </summary>
        public VersionInfoBuilder SetFileFlags(FileFlags fileFlags)
        {
            _fileFlags = fileFlags;
            return this;
        }

        /// <summary>
        /// Sets file operating system.
        /// </summary>
        public VersionInfoBuilder SetFileOperatingSystem(FileOperatingSystem fileOperatingSystem)
        {
            _fileOperatingSystem = fileOperatingSystem;
            return this;
        }

        /// <summary>
        /// Sets file type.
        /// </summary>
        public VersionInfoBuilder SetFileType(FileType fileType)
        {
            _fileType = fileType;
            return this;
        }

        /// <summary>
        /// Sets file sub-type.
        /// </summary>
        public VersionInfoBuilder SetFileSubType(FileSubType fileSubType)
        {
            _fileSubType = fileSubType;
            return this;
        }

        /// <summary>
        /// Sets file timestamp.
        /// </summary>
        public VersionInfoBuilder SetFileTimestamp(DateTimeOffset fileTimestamp)
        {
            _fileTimestamp = fileTimestamp;
            return this;
        }

        /// <summary>
        /// Removes all attributes.
        /// </summary>
        public VersionInfoBuilder ClearAttributes()
        {
            _attributes.Clear();
            return this;
        }

        /// <summary>
        /// Sets version attribute.
        /// </summary>
        public VersionInfoBuilder SetAttribute(VersionAttributeName name, string value)
        {
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Removes all translations.
        /// </summary>
        public VersionInfoBuilder ClearTranslations()
        {
            _translations.Clear();
            return this;
        }

        /// <summary>
        /// Adds a translation.
        /// </summary>
        public VersionInfoBuilder AddTranslation(TranslationInfo translationInfo)
        {
            _translations.Add(translationInfo);
            return this;
        }

        /// <summary>
        /// Adds a translation.
        /// </summary>
        public VersionInfoBuilder AddTranslation(int languageId, int codepage) =>
            AddTranslation(new TranslationInfo(languageId, codepage));

        /// <summary>
        /// Copies all data from an existing <see cref="VersionInfo"/> instance.
        /// </summary>
        public VersionInfoBuilder CopyFrom(VersionInfo versionInfo)
        {
            SetFileVersion(versionInfo.FileVersion);
            SetProductVersion(versionInfo.ProductVersion);
            SetFileFlags(versionInfo.FileFlags);
            SetFileOperatingSystem(versionInfo.FileOperatingSystem);
            SetFileType(versionInfo.FileType);
            SetFileSubType(versionInfo.FileSubType);
            SetFileTimestamp(versionInfo.FileTimestamp);

            ClearAttributes();
            foreach (var (name, value) in versionInfo.Attributes)
            {
                SetAttribute(name, value);
            }

            ClearTranslations();
            foreach (var translationInfo in versionInfo.Translations)
            {
                AddTranslation(translationInfo);
            }

            return this;
        }

        /// <summary>
        /// Builds a new <see cref="VersionInfo"/> instance.
        /// </summary>
        public VersionInfo Build() => new(
            _fileVersion,
            _productVersion,
            _fileFlags,
            _fileOperatingSystem,
            _fileType,
            _fileSubType,
            _fileTimestamp,
            _attributes,
            _translations
        );
    }
}