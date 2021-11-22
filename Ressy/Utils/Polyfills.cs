// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
using System.Collections.Generic;

internal static class PolyfillExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}
#endif