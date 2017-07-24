using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPSPHRUT
{
    public class DirectoryHelper
    {
        public static int CountFile(string path) =>
            Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;

        public static int RandomFile(string path) => Global.Random.Next(CountFile(path));
    }
}
