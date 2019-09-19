using ChummerHub.Data;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace ChummerHub
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Program'
    public class Program
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Program'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Program.MyHost'
        public static IWebHost MyHost = null;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Program.MyHost'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Program.Main(string[])'
        public static void Main(string[] args)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Program.Main(string[])'
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



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Program.CreateWebHostBuilder(string[])'
        public static IWebHost CreateWebHostBuilder(string[] args) =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Program.CreateWebHostBuilder(string[])'
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
