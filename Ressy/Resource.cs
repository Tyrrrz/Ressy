using System.Text;

namespace Ressy;

/// <summary>
/// Resource stored in a portable executable image.
/// </summary>
public class Resource
{
    /// <summary>
    /// Resource identifier.
    /// </summary>
    public ResourceIdentifier Identifier { get; }

    /// <summary>
    /// Binary data associated with the resource.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="Resource" />.
    /// </summary>
    public Resource(ResourceIdentifier identifier, byte[] data)
    {
        Identifier = identifier;
        Data = data;
    }

    /// <summary>
    /// Decodes resource binary data as a text string.
    /// Uses Unicode (UTF-16) encoding by default.
    /// </summary>
    public string ReadAsString(Encoding? encoding = null) =>
        (encoding ?? Encoding.Unicode).GetString(Data);
}