using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class ChummerHubVersion
    {
        public string AssemblyVersion { get; set; }

        public ChummerHubVersion()
        {
            AssemblyVersion = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }
    }
}

