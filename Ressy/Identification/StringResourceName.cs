using Ressy.Utils;

namespace Ressy.Identification
{
    internal class StringResourceName : ResourceName
    {
        private readonly string _name;

        public override int? Code => null;

        public override string Label => _name;

        public StringResourceName(string name) => _name = name;

        internal override IUnmanagedMemory ToUnmanagedMemory() =>
            new StringUnmanagedMemory(_name);
    }
}