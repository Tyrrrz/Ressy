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

namespace System
{
    internal static class HashCode
    {
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var hc1 = value1?.GetHashCode() ?? 0;
            var hc2 = value2?.GetHashCode() ?? 0;
            var hc3 = value3?.GetHashCode() ?? 0;

            unchecked
            {
                return (((hc1 * 397) ^ hc2) * 397) ^ hc3;
            }
        }
    }
}

namespace System.Collections.Generic
{
    internal static class PolyfillExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;
    }
}
#endif