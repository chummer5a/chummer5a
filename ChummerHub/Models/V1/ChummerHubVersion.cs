using System.Reflection;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion'
    public class ChummerHubVersion
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion.AssemblyVersion'
        public string AssemblyVersion { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion.AssemblyVersion'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion.ChummerHubVersion()'
        public ChummerHubVersion()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHubVersion.ChummerHubVersion()'
        {
            AssemblyVersion = typeof(Startup).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        }
    }
}

