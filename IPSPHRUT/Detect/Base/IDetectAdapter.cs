using System;
using System.Drawing;

namespace IPSPHRUT
{
    public class DetectAdapterEventAgrs : EventArgs
    {
        public FaceBase Face { private set; get; }
        public Exception Exception { private set; get; }

        public DetectAdapterEventAgrs(FaceBase face)
        {
            Face = face;
        }

        public DetectAdapterEventAgrs(Exception ex)
        {
            Exception = ex;
        }
    }

    interface IDetectAdapter
    {
        event EventHandler<DetectAdapterEventAgrs> OnFailure;
        event EventHandler<DetectAdapterEventAgrs> OnSuccess;
        void Process(Bitmap image, Action<double> report);
    }
}
