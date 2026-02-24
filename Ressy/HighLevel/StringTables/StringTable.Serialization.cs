using System;
using System.IO;

namespace Ressy.HighLevel.StringTables;

internal partial class StringTable
{
    internal static byte[] Serialize(string[] strings)
    {
        if (strings.Length != BlockSize)
            throw new ArgumentException(
                $"String table block must contain exactly {BlockSize} strings.",
                nameof(strings)
            );

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding);

        for (var i = 0; i < BlockSize; i++)
        {
            var str = strings[i];
            writer.Write((ushort)str.Length);
            foreach (var c in str)
                writer.Write(c);
        }

        return stream.ToArray();
    }
}
