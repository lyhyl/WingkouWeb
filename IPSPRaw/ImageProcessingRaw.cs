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

        public string Process(string uri) => Process(uri, null);

        public string Process(string uri, Action<double> callback)
        {
            callback?.Invoke(0);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)), oms = new MemoryStream())
            {
                callback?.Invoke(0.25);
                using (Bitmap img = new Bitmap(ms))
                {
                    callback?.Invoke(0.5);
                    using (Graphics g = Graphics.FromImage(img))
                        g.DrawString("Raw Plugin Test", SystemFonts.DefaultFont, Brushes.Gray, Point.Empty);
                    callback?.Invoke(0.75);
                    img.Save(oms, ImageFormat.Jpeg);
                    callback?.Invoke(1);
                    return Convert.ToBase64String(oms.ToArray());
                }
            }
        }
    }
}
