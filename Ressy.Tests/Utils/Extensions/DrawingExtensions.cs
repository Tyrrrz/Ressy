using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace Ressy.Tests.Utils.Extensions;

internal static class DrawingExtensions
{
    extension(Bitmap bitmap)
    {
        [SupportedOSPlatform("windows")]
        public byte[] GetData(ImageFormat format)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, format);

            return stream.ToArray();
        }

        [SupportedOSPlatform("windows")]
        public byte[] GetData() => bitmap.GetData(ImageFormat.Bmp);
    }
}
