// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective

#if NETSTANDARD2_0
internal static class PolyfillExtensions
{
    public static bool StartsWith(this string str, char c) => str.Length > 0 && str[0] == c;
}
#endif