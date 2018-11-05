using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ChummerHub.Services.GoogleDrive;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChummerHub
{
//#pragma warning disable CS1591
    public class Program
    {
        
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                /*.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5000);  // http:localhost:5000
                    options.Listen(IPAddress.Any, 80);         // http:*:80
                    options.Listen(IPAddress.Loopback, 443, listenOptions =>
                    {
                        listenOptions.UseHttps("certificate.pfx", "password");
                    });
                })*/
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                });
                
    }
//#pragma warning restore CS1591
}
