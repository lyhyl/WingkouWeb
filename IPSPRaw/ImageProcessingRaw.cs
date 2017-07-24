using ImageProcessingServicePlugin;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IPSPRaw
{
    public class ImageProcessingRaw : IIPSPlugin
    {
        public string Name => nameof(ImageProcessingRaw);

        public string Description => "Horizontal flip only";

        public void Dispose() { }

        public string Process(string uri)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)), oms = new MemoryStream())
            {
                using (Bitmap img = new Bitmap(ms))
                {
                    img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    img.Save(oms, ImageFormat.Jpeg);
                    return Convert.ToBase64String(oms.ToArray());
                }
            }
        }
    }
}
