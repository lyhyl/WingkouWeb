//using Affdex;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Drawing;
using System.Linq;

namespace IPSPHRUT
{
    public class MSFace : FaceBase
    {
        private Emotion emotion;
        private Microsoft.ProjectOxford.Face.Contract.Face face;
        private float[] marks;

        public MSFace(Microsoft.ProjectOxford.Face.Contract.Face f, Emotion e)
        {
            face = f;
            emotion = e;
            marks = new float[] {
                e.Scores.Anger * 100,
                e.Scores.Contempt * 100,
                e.Scores.Disgust * 100,
                e.Scores.Fear * 100,
                e.Scores.Happiness * 100,
                e.Scores.Sadness * 100,
                e.Scores.Surprise * 100,
            };
        }

        public override Age Age
        {
            get
            {
                if (face.FaceAttributes == null)
                    return Age.Age_Unknown;
                if (face.FaceAttributes.Age < 0)
                    return Age.Age_Unknown;
                if (face.FaceAttributes.Age < 18)
                    return Age.Age_Under_18;
                if (face.FaceAttributes.Age < 24)
                    return Age.Age_18_24;
                if (face.FaceAttributes.Age < 34)
                    return Age.Age_25_34;
                if (face.FaceAttributes.Age < 44)
                    return Age.Age_35_44;
                if (face.FaceAttributes.Age < 54)
                    return Age.Age_45_54;
                if (face.FaceAttributes.Age < 64)
                    return Age.Age_55_64;
                return Age.Age_65_Plus;
            }
        }

        public override float Anger => emotion.Scores.Anger * 100;
        public override float Contempt => emotion.Scores.Contempt * 100;
        public override float Disgust => emotion.Scores.Disgust * 100;
        public override float Fear => emotion.Scores.Fear * 100;
        public override float Joy => emotion.Scores.Happiness * 100;
        public override float Sadness => emotion.Scores.Sadness * 100;
        public override float Surprise => emotion.Scores.Surprise * 100;

        public override Emoji DominantEmoji => Emoji.Unknown;

        public override float DominantEmotion => marks.Max();

        public override int DominantEmotionIndex
        {
            get
            {
                float mx = DominantEmotion;
                if (mx < 1)
                    return -1;
                for (int i = 0; i < marks.Length; i++)
                    if (marks[i] == mx)
                        return i;
                throw new SystemException("Could not find max index");
            }
        }

        public override Ethnicity Ethnicity => Ethnicity.UNKNOWN;

        public override Gender Gender
        {
            get
            {
                if (face.FaceAttributes == null)
                    return Gender.Unknown;
                if (face.FaceAttributes.Gender.ToLower() == "female")
                    return Gender.Female;
                if (face.FaceAttributes.Gender.ToLower() == "male")
                    return Gender.Male;
                return Gender.Unknown;
            }
        }

        public override CircleF EnclosingCircle()
        {
            RectangleF rect = EnclosingRectangle();
            float r = (float)Math.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height) / 2;
            return new CircleF(
                new PointF(
                    (rect.Left + rect.Right) / 2,
                    (rect.Top + rect.Bottom) / 2),
                r);
        }

        public override RectangleF EnclosingRectangle()
        {
            return new RectangleF(
                emotion.FaceRectangle.Left, emotion.FaceRectangle.Top,
                emotion.FaceRectangle.Width, emotion.FaceRectangle.Height);
        }
    }
}
