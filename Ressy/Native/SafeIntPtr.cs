using System;
using System.Diagnostics.CodeAnalysis;

namespace Ressy.Native
{
    internal partial class SafeIntPtr : IDisposable
    {
        private readonly IntPtr _raw;
        private readonly Action<IntPtr> _dispose;

        public SafeIntPtr(IntPtr raw, Action<IntPtr> dispose)
        {
            _raw = raw;
            _dispose = dispose;
        }

        [ExcludeFromCodeCoverage]
        ~SafeIntPtr() => Dispose();

        public void Dispose() => _dispose(_raw);
    }

    internal partial class SafeIntPtr
    {
        public static implicit operator IntPtr(SafeIntPtr safeIntPtr) => safeIntPtr._raw;

        public static SafeIntPtr FromValue(int value) => new(new IntPtr(value), _ => { });
    }
}