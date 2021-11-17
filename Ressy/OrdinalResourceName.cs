using System.Globalization;
using Ressy.Native;

namespace Ressy
{
    internal class OrdinalResourceName : ResourceName
    {
        private readonly int _code;

        public override int? Code => _code;

        public override string Label => '#' + _code.ToString(CultureInfo.InvariantCulture);

        public OrdinalResourceName(int code) => _code = code;

        internal override SafeIntPtr ToPointer() => SafeIntPtr.FromValue(_code);
    }
}