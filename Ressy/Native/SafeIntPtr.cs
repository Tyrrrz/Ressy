using System;

namespace Ressy.Native
{
    internal readonly partial struct SafeIntPtr : IDisposable
    {
        private readonly IntPtr _raw;
        private readonly Action<IntPtr> _dispose;

        public SafeIntPtr(IntPtr raw, Action<IntPtr> dispose)
        {
            _raw = raw;
            _dispose = dispose;
        }

        public void Dispose() => _dispose(_raw);
    }

    internal partial struct SafeIntPtr
    {
        public static implicit operator IntPtr(SafeIntPtr safeIntPtr) => safeIntPtr._raw;

        public static SafeIntPtr FromValue(int value) => new(new IntPtr(value), _ => { });
    }
}