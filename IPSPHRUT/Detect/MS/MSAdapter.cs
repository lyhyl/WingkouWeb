using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IPSPHRUT
{
    public class MSAdapter : IDetectAdapter
    {
        public event EventHandler<DetectAdapterEventAgrs> OnFailure;
        public event EventHandler<DetectAdapterEventAgrs> OnSuccess;

        private const string faceApiRoot = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";
        private FaceServiceClient msfclient = new FaceServiceClient("f7860639317d427995a944c810ad9291");
        private EmotionServiceClient mseclient = new EmotionServiceClient("44da51c4461d4fa7b78808fc7769f349");

        public MSAdapter()
        {

        }

        public void Process(System.Drawing.Bitmap image)
        {
            try
            {
                //Logger.WriteLog($"MSCS process");
                using (MemoryStream ms0 = new MemoryStream(), ms1 = new MemoryStream())
                {
                    image.Save(ms0, ImageFormat.Jpeg);
                    image.Save(ms1, ImageFormat.Jpeg);
                    ms0.Position = 0;
                    ms1.Position = 0;

                    FaceAttributeType[] fat = { FaceAttributeType.Age, FaceAttributeType.Gender };
                    Face[] faces = AsyncHelper.RunSync(() => msfclient.DetectAsync(ms0, false, false, fat));
                    if (faces.Length == 0)
                    {
                        OnFailure?.Invoke(this, new DetectAdapterEventAgrs(new Exception("M$都找不到脸啊QAQ")));
                        return;
                    }
                    Face mxFace = faces[0];
                    for (int i = 1; i < faces.Length; i++)
                        if (faces[i].FaceRectangle.Width * faces[i].FaceRectangle.Height >
                            mxFace.FaceRectangle.Width * mxFace.FaceRectangle.Height)
                            mxFace = faces[i];
                    Rectangle[] rect = new Rectangle[] {
                        new Rectangle() {
                            Left = mxFace.FaceRectangle.Left,
                            Top = mxFace.FaceRectangle.Top,
                            Width = mxFace.FaceRectangle.Width,
                            Height = mxFace.FaceRectangle.Height
                        }
                    };

                    Emotion[] emotions = AsyncHelper.RunSync(() => mseclient.RecognizeAsync(ms1, rect));
                    if (emotions.Length == 0)
                    {
                        OnFailure?.Invoke(this, new DetectAdapterEventAgrs(new Exception("M$看不清脸啊QAQ")));
                        return;
                    }
                    OnSuccess?.Invoke(this, new DetectAdapterEventAgrs(new MSFace(mxFace, emotions[0])));
                }
            }
            catch (Exception ex)
            {
                OnFailure?.Invoke(this, new DetectAdapterEventAgrs(ex));
            }
        }
    }

    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
