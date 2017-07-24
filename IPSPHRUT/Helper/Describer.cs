//using Affdex;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPSPHRUT
{
    class Describer
    {
        private static string RawDataName(int idx)
        {
            string[] ns = {
                "Anger",
                "Contempt",
                "Disgust",
                "Fear",
                "Joy",
                "Sadness",
                "Surprise"
            };
            return ns[idx];
        }

        public static string DescribeFace(FaceBase face)
        {
            // know nothing
            if (face.Age == Age.Age_Unknown &&
                face.Gender == Gender.Unknown)
                return "此脸只应天上有~";

            // only gender
            if (face.Age == Age.Age_Unknown)
                return DescribeFaceOnlyGender(face);

            // only age
            if (face.Gender == Gender.Unknown)
                return DescribeFaceOnlyAge(face);

            // full info
            return DescribeFaceFullInfo(face);
        }

        private static string DescribeFaceFullInfo(FaceBase face)
        {
            if (face.Age == Age.Age_Under_18)
                return $"萌萌哒{(face.Gender == Gender.Female ? "萝莉" : "正太")}";
            if (face.Gender == Gender.Female)
            {
                string[] t = { "娇娥", "淑女", "裙钗", "罗敷" };
                return t[Global.Random.Next(t.Length)];
            }
            else
            {
                switch (face.Age)
                {
                    case Age.Age_18_24:
                        return "弱冠美男子";
                    case Age.Age_25_34:
                        return "而立男儿";
                    case Age.Age_35_44:
                        return "不惑汉子";
                    case Age.Age_45_54:
                        return "知命汉子";
                    case Age.Age_55_64:
                        return "花甲汉子";
                    case Age.Age_65_Plus:
                        return "古稀汉子";
                    default:
                    case Age.Age_Unknown:
                    case Age.Age_Under_18:
                        return "如果你看到这句话说明服务器炸了";
                }
            }
        }

        private static string DescribeFaceOnlyAge(FaceBase face)
        {
            switch (face.Age)
            {
                case Age.Age_Under_18:
                case Age.Age_18_24:
                    return "年轻就是好";
                case Age.Age_25_34:
                    return "要朝气蓬勃啊";
                case Age.Age_35_44:
                case Age.Age_45_54:
                    return "成熟稳重";
                case Age.Age_55_64:
                case Age.Age_65_Plus:
                    return "要神采奕奕哟";
                default:
                case Age.Age_Unknown:
                    return "如果你看到这句话说明服务器炸了";
            }
        }

        private static string DescribeFaceOnlyGender(FaceBase face)
        {
            if (face.Gender == Gender.Female)
                return "妹纸";
            else if (face.Gender == Gender.Male)
                return "汉子";
            else
                return "如果你看到这句话说明服务器炸了";
        }

        public static string DescribeIndex(FaceBase face)
        {
            string[] ns = {
                "好气啊!",
                "你看我理你?",
                "厌恶~",
                //"Engagement",
                "好方QAQ",
                "愉快~lalal~",
                "伤心T^T",
                "惊奇!",
                //"Valence"
            };
            int idx = face.DominantEmotionIndex;
            if (idx < 0)
                return "自然";
            return ns[idx];
        }

        public static string DescribEmoji(FaceBase face)
        {
            StringBuilder sb = new StringBuilder(@"Content\emojiData\");
            if (face.DominantEmoji != Emoji.Unknown)
            {
                sb.Append($@"emoji\{face.DominantEmoji}\");
            }
            else
            {
                sb.Append(@"emotion\");
                int idx = face.DominantEmotionIndex;
                if (idx < 0)
                    sb.Append(@"Null\");
                else
                    sb.Append($@"{RawDataName(idx)}\");
            }
            string path = Path.Combine(Global.PluginRoot, sb.ToString());
            sb.Append(DirectoryHelper.RandomFile(path));
            sb.Append(".png");
            return Path.Combine(Global.PluginRoot, sb.ToString());
        }

        public static string DescribeIndexDetail(FaceBase face)
        {
            string[] ns0 = {
                "生气指数有点高,生气显老啊!",
                "傲娇指数高企啊~哼~",
                "今天遇到\"不可描述\"的事情?",
                $"别{(face.Gender == Gender.Male ? "怂" : "怕")},我在!",
                "开心的你最可爱",
                "宝宝你别哭",
                "收到礼物了?"
            };
            string[] ns1 = {
                "生气指数有点高,好气啊!",
                "傲娇指数高企啊~哼~",
                "发生什么事情了O_O?",
                $"恐惧指数爆棚..好恐怖T^T",
                "猴开熏~~~",
                "伤心绝顶QwQ求抱抱",
                "Surprise!"
            };
            string[][] ns = { ns0, ns1 };
            int idx = face.DominantEmotionIndex;
            if (idx < 0)
                return "\"我的内心毫无波澜\"";
            return ns[Global.Random.Next(ns.Length)][idx];
        }

        public static Color EmotionColor(FaceBase face)
        {
            Color[] cs = {
                Color.FromArgb(218,83,44),
                Color.LightSlateGray,
                Color.FromArgb(255,153,180,51),
                Color.LightSalmon,
                Color.FromArgb(255,196,13),
                Color.FromArgb(43,87,151),
                Color.FromArgb(126,56,120)
            };
            int idx = face.DominantEmotionIndex;
            if (idx < 0)
                return Color.FromArgb(0, 171, 169);
            return cs[idx];
        }

        public static Color GenderColor(FaceBase face)
        {
            switch (face.Gender)
            {
                default:
                case Gender.Unknown:
                    return Color.Black;
                case Gender.Male:
                    return Color.DodgerBlue;
                case Gender.Female:
                    return Color.Plum;
            }
        }

        public static string Avatar(FaceBase face)
        {
            if (face.Gender == Gender.Unknown || face.Age == Age.Age_Unknown)
                return string.Empty;
            StringBuilder path = new StringBuilder($@"Content\emojiData\avatar\{face.Gender.ToString()}\");
            switch (face.Age)
            {
                default:
                case Age.Age_Under_18:
                case Age.Age_18_24:
                    path.Append(@"0-24\");
                    break;
                case Age.Age_25_34:
                case Age.Age_35_44:
                case Age.Age_45_54:
                    path.Append(@"25-54\");
                    break;
                case Age.Age_55_64:
                case Age.Age_65_Plus:
                    path.Append(@"55+\");
                    break;
            }
            path.Append(face.Ethnicity.ToString());
            path.Append(".png");
            return Path.Combine(Global.PluginRoot, path.ToString());
        }
    }
}
