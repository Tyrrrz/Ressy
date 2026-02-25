using System.Text;

namespace Ressy;

/// <summary>
/// Resource stored in a portable executable image.
/// </summary>
public class Resource(ResourceIdentifier identifier, byte[] data)
{
    /// <summary>
    /// Resource identifier.
    /// </summary>
    public ResourceIdentifier Identifier { get; } = identifier;

    /// <summary>
    /// Binary data associated with the resource.
    /// </summary>
    public byte[] Data { get; } = data;

    /// <summary>
    /// Decodes resource binary data as a text string.
    /// </summary>
    /// <remarks>
    /// If encoding is not specified, Unicode (UTF-16) is used by default.
    /// </remarks>
    public string ReadAsString(Encoding? encoding = null) =>
        (encoding ?? Encoding.Unicode).GetString(Data);
}
