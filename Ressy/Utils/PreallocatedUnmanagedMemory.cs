﻿using System;

namespace Ressy.Utils
{
    internal class PreallocatedUnmanagedMemory : IUnmanagedMemory
    {
        public IntPtr Handle { get; }

        public PreallocatedUnmanagedMemory(IntPtr handle) => Handle = handle;

        public PreallocatedUnmanagedMemory(int handleValue)
            : this(new IntPtr(handleValue))
        {
        }

        public void Dispose()
        {
            // Nothing to free up because we don't own the memory
        }
    }
}