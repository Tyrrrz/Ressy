using System;

namespace Ressy.Native;

internal interface INativeHandle : IDisposable
{
    IntPtr Value { get; }
}