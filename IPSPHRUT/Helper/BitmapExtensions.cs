//using Affdex;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IPSPHRUT
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap">bitmap *MUST* be 24bppRgb</param>
        /// <returns>Frame</returns>
        //public static Frame ConvertToFrame(this Bitmap bitmap)
        //{
        //    if (bitmap == null)
        //        throw new ArgumentNullException(nameof(bitmap));
        //    if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
        //        throw new ArgumentException($"{nameof(bitmap)} require {nameof(PixelFormat.Format24bppRgb)}");

        //    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        //    BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

        //    IntPtr ptr = bmpData.Scan0;

        //    int numBytes = bitmap.Width * bitmap.Height * 3;
        //    byte[] rgbValues = new byte[numBytes];

        //    int data_x = 0;
        //    int ptr_x = 0;
        //    int row_bytes = bitmap.Width * 3;

        //    for (int y = 0; y < bitmap.Height; y++)
        //    {
        //        Marshal.Copy(ptr + ptr_x, rgbValues, data_x, row_bytes);
        //        data_x += row_bytes;
        //        ptr_x += bmpData.Stride;
        //    }

        //    bitmap.UnlockBits(bmpData);

        //    return new Frame(bitmap.Width, bitmap.Height, rgbValues, Frame.COLOR_FORMAT.BGR);
        //}

        public static Bitmap LoadImageFitSize(string fileName, int mxl = 1024)
        {
            using (Bitmap org = new Bitmap(fileName))
            {
                RotateImage(org);
                Size size = org.Size;
                double mxlen = mxl;
                int len = Math.Max(size.Width, size.Height);
                double scale = mxlen / len;
                if (len > mxlen)
                {
                    size.Width = (int)(size.Width * scale);
                    size.Height = (int)(size.Height * scale);
                    Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(org,
                            new RectangleF(0, 0, (float)(org.Width * scale), (float)(org.Height * scale)),
                            new RectangleF(0, 0, org.Width, org.Height),
                            GraphicsUnit.Pixel);
                    }
                    return bmp;
                }
                else
                {
                    Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                        g.DrawImagePixel(org);
                    return bmp;
                }
            }
        }

        private static void RotateImage(Image img)
        {
            if (img.RawFormat != ImageFormat.Jpeg)
                return;
            const int exif = 0x0112;
            if (Array.IndexOf(img.PropertyIdList, exif) > -1)
            {
                switch (img.GetPropertyItem(exif).Value[0])
                {
                    case 1: /* No rotation required.*/ break;
                    case 2: img.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                    case 3: img.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 4: img.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                    case 5: img.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                    case 6: img.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 7: img.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                    case 8: img.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }
                img.RemovePropertyItem(exif);
            }
        }

        public static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
                if (codec.FormatID == format.Guid)
                    return codec;
            return null;
        }
    }
}
