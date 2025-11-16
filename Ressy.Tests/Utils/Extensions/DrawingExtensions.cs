using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ressy.Tests.Utils.Extensions;

internal static class DrawingExtensions
{
    extension(Bitmap bitmap)
    {
        public byte[] GetData(ImageFormat format)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, format);

            return stream.ToArray();
        }

        public byte[] GetData() => bitmap.GetData(ImageFormat.Bmp);
    }
}
