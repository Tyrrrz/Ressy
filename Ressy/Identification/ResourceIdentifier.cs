namespace Ressy.Identification
{
    public class ResourceIdentifier
    {
        public ResourceType Type { get; }

        public ResourceName Name { get; }

        public ResourceLanguage Language { get; }

        public ResourceIdentifier(ResourceType type, ResourceName name, ResourceLanguage language)
        {
            Type = type;
            Name = name;
            Language = language;
        }

        public ResourceIdentifier(ResourceType type, ResourceName name)
            : this(type, name, ResourceLanguage.Neutral)
        {
        }

        public override string ToString() => $"{Type} / {Name} / {Language}";
    }
}