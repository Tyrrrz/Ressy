// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
namespace System.Collections.Generic
{
    internal static class PolyfillExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary,
            TKey key) =>
            dictionary.TryGetValue(key, out var value) ? value : default;
    }
}
#endif