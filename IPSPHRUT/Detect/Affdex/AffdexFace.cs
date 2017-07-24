//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Affdex;

//namespace IPSPHRUT
//{
//    public class AffdexFace : FaceBase
//    {
//        private Face face;
//        private float[] marks;

//        public AffdexFace(Face face)
//        {
//            this.face = face;
//            marks = new float[] {
//                face.Emotions.Anger,
//                face.Emotions.Contempt,
//                face.Emotions.Disgust,
//                //face.Emotions.Engagement,
//                face.Emotions.Fear,
//                face.Emotions.Joy,
//                face.Emotions.Sadness,
//                face.Emotions.Surprise,
//                //face.Emotions.Valence
//            };
//        }

//        public override Gender Gender => face.Appearance.Gender;

//        public override float Anger => face.Emotions.Anger;
//        public override float Contempt => face.Emotions.Contempt;
//        public override float Disgust => face.Emotions.Disgust;
//        public override float Fear => face.Emotions.Fear;
//        public override float Joy => face.Emotions.Joy;
//        public override float Sadness => face.Emotions.Sadness;
//        public override float Surprise => face.Emotions.Surprise;

//        public override Ethnicity Ethnicity => face.Appearance.Ethnicity;

//        public override Age Age => face.Appearance.Age;

//        public override Emoji DominantEmoji => face.Emojis.dominantEmoji;

//        public override float DominantEmotion => marks.Max();

//        public override int DominantEmotionIndex
//        {
//            get
//            {
//                float mx = DominantEmotion;
//                if (mx < 1)
//                    return -1;
//                for (int i = 0; i < marks.Length; i++)
//                    if (marks[i] == mx)
//                        return i;
//                throw new SystemException("Could not find max index");
//            }
//        }

//        public override CircleF EnclosingCircle()
//        {
//            RectangleF rect = EnclosingRectangle();
//            float r = (float)Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height) / 2;
//            return new CircleF(
//                new PointF(
//                    (rect.Left + rect.Right) / 2,
//                    (rect.Top + rect.Bottom) / 2),
//                r);
//        }

//        public override RectangleF EnclosingRectangle()
//        {
//            var pts = face.FeaturePoints;
//            float left = pts.Min(p => p.X);
//            float right = pts.Max(p => p.X);
//            float top = pts.Min(p => p.Y);
//            float bottom = pts.Max(p => p.Y);
//            return new RectangleF(left, top, right - left, bottom - top);
//        }
//    }
//}
