using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ChummerHub.Areas.Identity.IdentityHostingStartup))]
namespace ChummerHub.Areas.Identity
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IdentityHostingStartup'
    public class IdentityHostingStartup : IHostingStartup
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IdentityHostingStartup'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IdentityHostingStartup.Configure(IWebHostBuilder)'
        public void Configure(IWebHostBuilder builder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IdentityHostingStartup.Configure(IWebHostBuilder)'
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}