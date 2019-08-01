using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore;
using ChummerHub.Data;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.DataContracts;

namespace ChummerHub
{
    public class Program
    {
        public static IWebHost MyHost = null;
        public static void Main(string[] args)
        {

            
          
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                //.WriteTo.File(@"ChummerHub_log.txt")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
#else
            System.AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
#endif

            MyHost = CreateWebHostBuilder(args);
            Seed();
            MyHost.Run();
            return;
        }

        private static bool _preventOverflow = false;
       
        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            try
            {
                if (_preventOverflow)
                    return;
                _preventOverflow = true;
                string msg = e.Exception.ToString() + Environment.NewLine + Environment.NewLine;

                if (!e.Exception.Message.Contains("Non-static method requires a target."))
                {
                    Console.WriteLine(msg);
                    //System.Diagnostics.Trace.TraceError(msg, e.Exception);
                    System.Diagnostics.Debug.WriteLine(msg);
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e.Exception);
                    tc.TrackException(et);
                }
                else
                {
                    Console.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.ToString() + Environment.NewLine + Environment.NewLine;
                Console.WriteLine(msg);
                System.Diagnostics.Debug.WriteLine(msg);
                //AggregateException ae = new AggregateException(new List<Exception>() { e.Exception, ex });
                //throw ae;
            }
            finally
            {
                _preventOverflow = false;
            }
        }

        public static void Seed()
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
                    try
                    {
                        var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                        var telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                        tc.TrackException(telemetry);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString());
                    }
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
                    var env = services.GetService<IHostingEnvironment>();
                    SeedData.Initialize(services, testUserPw, env).Wait();
                }
                catch(Exception ex)
                {
                    try
                    {
                        var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                        var telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                        tc.TrackException(telemetry);
                    }
                    catch (Exception e1)
                    {
                        logger.LogError(e1.ToString());
                    }
                    logger.LogError(ex.Message, "An error occurred seeding the DB: " + ex.ToString());
                }
            }

        }

        public static IWebHost CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
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
                .UseIISIntegration()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .CaptureStartupErrors(true)
                .Build();
    }
}
