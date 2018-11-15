// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore;

namespace ChummerHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(@"ChummerHub_log.txt")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            var seed = args.Contains("/seed");
            if (seed)
            {
                args = args.Except(new[] { "/seed" }).ToArray();
            }

            var host = CreateWebHostBuilder(args).Build();
            
            if (seed)
            {
                //SeedData.EnsureSeedData(host.Services);
            }

            host.Run();
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

        //public static IWebHost BuildWebHost(string[] args)
        //{
        //    return WebHost.CreateDefaultBuilder(args)
        //            .UseStartup<Startup>()
        //            .UseSerilog((context, configuration) =>
        //            {
        //                configuration
        //                    .MinimumLevel.Debug()
        //                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        //                    .MinimumLevel.Override("System", LogEventLevel.Warning)
        //                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        //                    .Enrich.FromLogContext()
        //                    .WriteTo.File(@"identityserver4_log.txt")
        //                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
        //            })
        //            .Build();
        //}
    }
}
