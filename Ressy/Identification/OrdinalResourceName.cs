using System;
using System.Globalization;
using Ressy.Utils;

namespace Ressy.Identification
{
    internal class OrdinalResourceName : ResourceName
    {
        private readonly int _code;

        public override int? Code => _code;

        public override string Label => '#' + _code.ToString(CultureInfo.InvariantCulture);

        public OrdinalResourceName(int code) => _code = code;

        internal override IUnmanagedMemory ToUnmanagedMemory() =>
            new PreallocatedUnmanagedMemory(new IntPtr(_code));
    }
}