using ImageProcessingServicePlugin;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IPSPHRUT
{
    public class ImageProcessingHRUT : IIPSPlugin
    {
        public string Name => nameof(ImageProcessingHRUT);

        public string Description => "How Are U Today?";

        private string Result = string.Empty;

        public void Dispose()
        {
        }

        public string Process(string uri)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)))
            {
                using (Bitmap img = new Bitmap(ms))
                {
                    PhotoHandler ph = new PhotoHandler();
                    ph.Detect(img, callback);
                }
            }
            return Result;
        }

        private void callback(object sender, PhotoHandlerEventArgs e)
        {
            if (e.Exception == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ImageCodecInfo jpgEncoder = BitmapExtensions.GetEncoderInfo(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);
                    encoderParams.Param[0] = encoderParam;
                    e.Image.Save(ms, jpgEncoder, encoderParams);
                    Result = Convert.ToBase64String(ms.ToArray());
                }
                //HandledImage.ImageUrl = "data:image/jpeg;base64," + base64String;
            }
            else
            {
                //Logger.WriteLog(nameof(PHandler_OnFinished), e.Exception);
                //HandledImage.ImageUrl = "Content/ui/faceNotFound.jpg";
                //HandledMessage.Text = e.Exception.Message;
                using (MemoryStream ms = new MemoryStream())
                {
                    ImageCodecInfo jpgEncoder = BitmapExtensions.GetEncoderInfo(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);
                    encoderParams.Param[0] = encoderParam;
                    using (Bitmap img = new Bitmap(Path.Combine(Global.PluginRoot, "Content/ui/faceNotFound.jpg")))
                    {
                        img.Save(ms, jpgEncoder, encoderParams);
                    }
                    Result = Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
