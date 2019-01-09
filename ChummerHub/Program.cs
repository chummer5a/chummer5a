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
using ChummerHub.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ChummerHub
{
    public class Program
    {
        public static IWebHost MyHost = null;
        public static void Main(string[] args)
        {

            MyHost = CreateWebHostBuilder(args).Build();
            InitializeDatabase();
            MyHost.Run();
            return;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                //.WriteTo.File(@"ChummerHub_log.txt")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            //var seed = args.Contains("/seed");
            //if (seed)
            //{
            //    args = args.Except(new[] { "/seed" }).ToArray();
            //}

            //var host = CreateWebHostBuilder(args).Build();
            
            //if (seed)
            //{
            //    //SeedData.EnsureSeedData(host.Services);
            //}

            //host.Run();
        }

        public static void InitializeDatabase()
        {
            
            using(var scope = MyHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch(Exception e)
                {
                    logger.LogError(e.Message, "An error occurred migrating the DB: " + e.ToString());
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
                // requires using Microsoft.Extensions.Configuration;
                var config = services.GetRequiredService<IConfiguration>();
                // Set password with the Secret Manager tool.
                // dotnet user-secrets set SeedUserPW <pw>
                var testUserPw = config["SeedUserPW"];
                try
                {
                    SeedData.Initialize(services, testUserPw).Wait();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex.Message, "An error occurred seeding the DB: " + ex.ToString());
                }
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Limits.MinResponseDataRate = null;
                    //options.Listen(IPAddress.Loopback, 5000);  // http:localhost:5000
                    //options.Listen(IPAddress.Any, 80);         // http:*:80
                    //options.Listen(IPAddress.Loopback, 443, listenOptions =>
                    //{
                    //    listenOptions.UseHttps("certificate.pfx", "password");
                    //});
                })
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                });

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Warning)
                            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                            .Enrich.FromLogContext()
#if DEBUG
                            .WriteTo.File(@"SINners_log.txt")
#endif
                            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                    })
                    .Build();
        }
    }
}
