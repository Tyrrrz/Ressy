using System;
using System.Collections.Generic;
using System.Linq;

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
        private FileType _fileType = FileType.Application;
        private FileSubType _fileSubType = FileSubType.Unknown;

        private readonly Dictionary<(Language, CodePage), Dictionary<VersionAttributeName, string>> _attributeTables =
            new();

        private Dictionary<VersionAttributeName, string> GetAttributeTable(Language language, CodePage codePage) =>
            _attributeTables.TryGetValue((language, codePage), out var table)
                ? table
                : _attributeTables[(language, codePage)] = new();

        /// <summary>
        /// Sets file version.
        /// </summary>
        public VersionInfoBuilder SetFileVersion(Version fileVersion, bool setAttribute = true)
        {
            _fileVersion = fileVersion;

            if (setAttribute)
                SetAttribute(VersionAttributeName.FileVersion, fileVersion.ToString(4));

            return this;
        }

        /// <summary>
        /// Sets product version.
        /// </summary>
        public VersionInfoBuilder SetProductVersion(Version productVersion, bool setAttribute = true)
        {
            _productVersion = productVersion;

            if (setAttribute)
                SetAttribute(VersionAttributeName.ProductVersion, productVersion.ToString(4));

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
        /// Removes all attribute tables.
        /// </summary>
        public VersionInfoBuilder ClearAttributes()
        {
            _attributeTables.Clear();
            return this;
        }

        /// <summary>
        /// Sets version attribute in the table bound to the specified language and code page.
        /// </summary>
        public VersionInfoBuilder SetAttribute(
            VersionAttributeName name,
            string value,
            Language language,
            CodePage codePage)
        {
            GetAttributeTable(language, codePage)[name] = value;
            return this;
        }

        /// <summary>
        /// Sets version attribute in all tables.
        /// If there are no existing tables, creates a new one bound to the neutral language and Unicode code page.
        /// </summary>
        public VersionInfoBuilder SetAttribute(VersionAttributeName name, string value)
        {
            // If tables already exist, set attribute in all of them
            if (_attributeTables.Any())
            {
                foreach (var (language, codepage) in _attributeTables.Keys)
                {
                    SetAttribute(name, value, language, codepage);
                }
            }
            // Otherwise, create a default table
            else
            {
                SetAttribute(name, value, Language.Neutral, CodePage.Unicode);
            }

            return this;
        }

        /// <summary>
        /// Copies all data from an existing <see cref="VersionInfo"/> instance.
        /// </summary>
        public VersionInfoBuilder SetAll(VersionInfo versionInfo)
        {
            SetFileVersion(versionInfo.FileVersion, false);
            SetProductVersion(versionInfo.ProductVersion, false);
            SetFileFlags(versionInfo.FileFlags);
            SetFileOperatingSystem(versionInfo.FileOperatingSystem);
            SetFileType(versionInfo.FileType);
            SetFileSubType(versionInfo.FileSubType);

            ClearAttributes();

            foreach (var attributeTable in versionInfo.AttributeTables)
            {
                foreach (var (name, value) in attributeTable.Attributes)
                {
                    SetAttribute(name, value, attributeTable.Language, attributeTable.CodePage);
                }
            }

            return this;
        }

        /// <summary>
        /// Builds a new <see cref="VersionInfo"/> instance.
        /// </summary>
        public VersionInfo Build()
        {
            // Make sure "FileVersion" and "ProductVersion" are set in every table.
            // Don't overwrite user-set values because they may contain extended version information.
            foreach (var attributeTable in _attributeTables.Values)
            {
                if (!attributeTable.ContainsKey(VersionAttributeName.FileVersion))
                    attributeTable[VersionAttributeName.FileVersion] = _fileVersion.ToString(4);

                if (!attributeTable.ContainsKey(VersionAttributeName.ProductVersion))
                    attributeTable[VersionAttributeName.ProductVersion] = _productVersion.ToString(4);
            }

            return new VersionInfo(
                _fileVersion,
                _productVersion,
                _fileFlags,
                _fileOperatingSystem,
                _fileType,
                _fileSubType,
                _attributeTables
                    .Select(t => new VersionAttributeTable(t.Key.Item1, t.Key.Item2, t.Value))
                    .ToArray()
            );
        }
    }
}