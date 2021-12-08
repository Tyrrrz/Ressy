using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ressy.Tests.Utils.Extensions;

internal static class DrawingExtensions
{
    public static byte[] GetData(this Bitmap bitmap, ImageFormat format)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, format);

        return stream.ToArray();
    }

    public static byte[] GetData(this Bitmap bitmap) =>
        bitmap.GetData(ImageFormat.Bmp);
}