using System;
using System.IO;
using System.Reflection;

namespace IPSPHRUT
{
    public class Global
    {
        public static string PluginRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static Random Random = new Random();
    }
}
