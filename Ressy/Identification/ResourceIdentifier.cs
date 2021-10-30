using System.Diagnostics.CodeAnalysis;

namespace Ressy.Identification
{
    /// <summary>
    /// Identifies a single resource stored in a portable executable image.
    /// </summary>
    public class ResourceIdentifier
    {
        /// <summary>
        /// Resource type.
        /// </summary>
        public ResourceType Type { get; }

        /// <summary>
        /// Resource name.
        /// </summary>
        public ResourceName Name { get; }

        /// <summary>
        /// Resource language.
        /// </summary>
        public ResourceLanguage Language { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ResourceIdentifier"/>.
        /// </summary>
        public ResourceIdentifier(ResourceType type, ResourceName name, ResourceLanguage language)
        {
            Type = type;
            Name = name;
            Language = language;
        }

        /// <summary>
        /// Initializes an instance of <see cref="ResourceIdentifier"/>.
        /// </summary>
        public ResourceIdentifier(ResourceType type, ResourceName name)
            : this(type, name, ResourceLanguage.Neutral)
        {
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Type} / {Name} / {Language}";
    }
}