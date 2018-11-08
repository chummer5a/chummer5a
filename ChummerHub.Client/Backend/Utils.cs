using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    public static class StaticUtils
    {
        public static Utils MyUtils = new Utils();
        static StaticUtils()
        {
        }
        public static bool IsUnitTest { get { return MyUtils.IsUnitTest; } }
    }

    public class Utils
    {
        public Utils()
        {
            IsUnitTest = false;
        }

        public bool IsUnitTest { get; set; }
    }
}
