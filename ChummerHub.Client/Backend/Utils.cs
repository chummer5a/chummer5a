using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    public static class Utils
    {
        static Utils()
        {
            string testAssemblyName = "Microsoft.TestPlatform.PlatformAbstractions";//"Microsoft.VisualStudio.QualityTools.UnitTestFramework";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            Utils.IsInUnitTest = assemblies.Any(a => a.FullName.StartsWith(testAssemblyName));
        }
        public static bool IsInUnitTest { get; private set; }
    }
}
