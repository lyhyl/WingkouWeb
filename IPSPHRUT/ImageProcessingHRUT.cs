using ImageProcessingServicePlugin;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IPSPHRUT
{
    public class ImageProcessingHRUT : IIPSPlugin
    {
        public string Name => nameof(ImageProcessingHRUT);

        public string Description => "How Are U Today?";

        private string Result = string.Empty;

        private Bitmap imageToProcess;
        private IDetectAdapter[] detectors;

        public void Dispose() { }

        public string Process(string uri) => Process(uri, null);

        public string Process(string uri, Action<double> reportCallback)
        {
            double len = 0.8;

            detectors = new IDetectAdapter[] {
                //AffdexAdapter.CreateSingleton(FaceDetectorMode.LARGE_FACES),
                new MSAdapter()
            };

            for (int i = 0; i < detectors.Length; i++)
                detectors[i].OnSuccess += Det_OnSuccess;

            for (int i = 0; i < detectors.Length - 1; i++)
            {
                int cur = i;
                int next = i + 1;
                detectors[i].OnFailure += (s, e) => detectors[next].Process(imageToProcess,
                    (p) => reportCallback?.Invoke(len * ((p + cur) / detectors.Length)));
            }

            detectors.Last().OnFailure += Det_OnFailure;

            reportCallback?.Invoke(0);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)))
            {
                reportCallback?.Invoke(0.1);
                using (Bitmap img = new Bitmap(ms))
                {
                    reportCallback?.Invoke(1- len);
                    imageToProcess = img;
                    detectors.First().Process(img, (p) => reportCallback?.Invoke(len * (p / detectors.Length)));
                }
            }
            reportCallback?.Invoke(1);
            return Result;
        }

        private void Det_OnFailure(object sender, DetectAdapterEventAgrs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = BitmapExtensions.GetEncoderInfo(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, 75L);
                encoderParams.Param[0] = encoderParam;
                using (Bitmap img = new Bitmap(Path.Combine(Global.PluginRoot, "Content/ui/faceNotFound.jpg")))
                    img.Save(ms, jpgEncoder, encoderParams);
                Result = Convert.ToBase64String(ms.ToArray());
            }
        }

        private void Det_OnSuccess(object sender, DetectAdapterEventAgrs e)
        {
            using (FaceDrawer faceDrawer = new FaceDrawer())
            using (Image image = faceDrawer.Draw(e.Face, imageToProcess))
            using (MemoryStream ms = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = BitmapExtensions.GetEncoderInfo(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);
                EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, 75L);
                encoderParams.Param[0] = encoderParam;
                image.Save(ms, jpgEncoder, encoderParams);
                Result = Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
