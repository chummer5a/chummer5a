/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using Azure.Identity;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Configuration;

namespace ChummerHub
{
    public class Program
    {
        public static IWebHost MyHost;
        public static void Main(string[] args)
        {



#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
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
        }

        private static bool _preventOverflow;

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            try
            {
                if (_preventOverflow)
                    return;
                _preventOverflow = true;
                string msg = e.Exception + Environment.NewLine + Environment.NewLine;

                if (!e.Exception.Message.Contains("Non-static method requires a target."))
                {
                    Console.WriteLine(msg);
                    System.Diagnostics.Trace.TraceError(msg, e.Exception);
                    System.Diagnostics.Debug.WriteLine(msg);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    //ExceptionTelemetry et = new ExceptionTelemetry(e.Exception);
                    //tc.TrackException(et);
                }
                else
                {
                    Console.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = ex + Environment.NewLine + Environment.NewLine;
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



        public static IWebHost CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            //.ConfigureAppConfiguration((hostingContext, config) =>
            //{
            //    var settings = config.Build();
            //    config.AddAzureAppConfiguration(options =>
            //    {
                    
            //        //options.Connect(settings["ConnectionStrings.AppConfig"])
            //        //        .ConfigureKeyVault(kv =>
            //        //        {
            //        //            kv.SetCredential(new DefaultAzureCredential());
            //        //        });
            //    });
            //})
                //.UseKestrel(options =>
                //{
                //    options.Limits.MinResponseDataRate = null;
                //    //options.Listen(IPAddress.Loopback, 5000);  // http:localhost:5000
                //    //options.Listen(IPAddress.Any, 80);         // http:*:80
                //    //options.Listen(IPAddress.Loopback, 443, listenOptions =>
                //    //{
                //    //    listenOptions.UseHttps("certificate.pfx", "password");
                //    //});
                //})
                .UseAzureAppServices()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseIISIntegration()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddApplicationInsights("95c486ab-aeb7-4361-8667-409b7bf62713");
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                    // Additional filtering For category starting in "Microsoft",
                    // only Warning or above will be sent to Application Insights.
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
                })
          
                .CaptureStartupErrors(true)
                .Build();
    }
}
