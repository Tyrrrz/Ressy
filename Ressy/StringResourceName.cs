using Ressy.Native;

namespace Ressy
{
    internal class StringResourceName : ResourceName
    {
        private readonly string _name;

        public override int? Code => null;

        public override string Label => _name;

        public StringResourceName(string name) => _name = name;

        internal override SafeIntPtr ToPointer() => SafeMarshal.AllocHGlobal(_name);
    }
}