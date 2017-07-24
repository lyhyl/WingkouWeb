//using Affdex;
using System.Drawing;

namespace IPSPHRUT
{
    public struct CircleF
    {
        public PointF Center;
        public float Radius;

        public CircleF(PointF c, float r)
        {
            Center = c;
            Radius = r;
        }
    }

    public enum Emoji
    {
        Relaxed = 9786,
        Smiley = 128515,
        Laughing = 128518,
        Wink = 128521,
        Smirk = 128527,
        Unknown = 128528,
        Kissing = 128535,
        StuckOutTongue = 128539,
        StuckOutTongueWinkingEye = 128540,
        Disappointed = 128542,
        Rage = 128545,
        Scream = 128561,
        Flushed = 128563
    }

    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }

    public enum Age
    {
        Age_Unknown = 0,
        Age_Under_18 = 1,
        Age_18_24 = 2,
        Age_25_34 = 3,
        Age_35_44 = 4,
        Age_45_54 = 5,
        Age_55_64 = 6,
        Age_65_Plus = 7
    }

    public enum Ethnicity
    {
        UNKNOWN = 0,
        CAUCASIAN = 1,
        BLACK_AFRICAN = 2,
        SOUTH_ASIAN = 3,
        EAST_ASIAN = 4,
        HISPANIC = 5
    }

    public abstract class FaceBase
    {
        //"neutral"
        //"Engagement"
        //"Valence"

        public abstract float Anger { get; }
        public abstract float Contempt { get; }
        public abstract float Disgust { get; }
        public abstract float Fear { get; }
        public abstract float Joy { get; }
        public abstract float Sadness { get; }
        public abstract float Surprise { get; }

        public abstract float DominantEmotion { get; }
        public abstract int DominantEmotionIndex { get; }

        public abstract Emoji DominantEmoji { get; }

        public abstract Gender Gender { get; }
        public abstract Age Age { get; }
        public abstract Ethnicity Ethnicity { get; }

        public abstract CircleF EnclosingCircle();
        public abstract RectangleF EnclosingRectangle();
    }
}
