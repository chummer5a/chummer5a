using IdentityModel.OidcClient.Browser;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ChummerHub.Client.OidcClient
{
    public class SystemBrowser : IBrowser
    {
        public int Port { get; }
        private readonly string _path;

        public SystemBrowser(int? port = null, string path = null)
        {
            _path = path;

            if (!port.HasValue)
            {
                Port = GetPsydoRandomUnusedPort();
            }
            else
            {
                Port = port.Value;
            }
        }

        private int GetPsydoRandomUnusedPort()
        {
            int[] allowedports = new int[3] { 5013, 64888, 62777 };
            int port = -1;
            int counter = -1;
            while (port == -1 && counter < 3)
            {
                counter++;
                try
                {
                    var listener = new TcpListener(IPAddress.Loopback, allowedports[counter]);
                    listener.Start();
                    port = ((IPEndPoint)listener.LocalEndpoint).Port;
                    listener.Stop();
                    return port;
                }
                catch (Exception e)
                {
                    port = -1;
                }
            }
            return -1;
            
            
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            using (var listener = new LoopbackHttpListener(Port, _path))
            {
                OpenBrowser(options.StartUrl);

                try
                {
                    var result = await listener.WaitForCallbackAsync();
                    if (String.IsNullOrWhiteSpace(result))
                    {
                        return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
                    }

                    return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
                }
                catch (TaskCanceledException ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                }
                catch (Exception ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
                }
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }


    public class LoopbackHttpListener : IDisposable
    {
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        

        string _url;

        public string Url => _url;

        public LoopbackHttpListener(int port, string path = null)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            List<SimpleHttpServer.Models.Route> route_config = new List<SimpleHttpServer.Models.Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = null,//@"^/$",
                    Method = "GET",
                    
                    Callable = (SimpleHttpServer.Models.HttpRequest request) =>
                    {
                        _source.TrySetResult(request.Path);
                        var result = new SimpleHttpServer.Models.HttpResponse()
                        {
                            ContentAsUTF8 = "Authentication complete. You may close this window now.",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                        string token;
                        if (request.Headers.TryGetValue("Authorization", out token))
                        {
                            result.Headers.Add("Authorization", token);
                        }
                        return result;
                     }
                },
                //new Route {
                //    Name = "FileSystem Static Handler",
                //    UrlRegex = path,
                //    Method = "GET",
                //    Callable = new SimpleHttpServer.RouteHandlers.FileSystemRouteHandler()
                //    {
                //        BasePath = @"C:\Tmp", ShowDirectories=true }.Handle,
                //    },
            };

            httpServer = new HttpServer(port, route_config);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
            //wait a bit for the httplistener to spin up
            System.Threading.Thread.Sleep(100);

        }

        private HttpServer httpServer;
        TaskCompletionSource<string> _source = new TaskCompletionSource<string>();
        //string _url;

        //public string Url => _url;

        //public LoopbackHttpListener(int port, string path = null)
        //{
        //    path = path ?? String.Empty;
        //    if (path.StartsWith("/")) path = path.Substring(1);

        //    _url = $"http://localhost:{port}/{path}";

        //    _host = new WebHostBuilder()
        //        .UseKestrel()
        //        .UseUrls(_url)
        //        .Configure(Configure)
        //        .Build();
        //    _host.Start();
        //}

        public void Dispose()
        {
            //Task.Run(async () =>
            //{
            //    await Task.Delay(500);
            //    _host.Dispose();
            //});
        }

        //void Configure(IApplicationBuilder app)
        //{
        //    app.Run(async ctx =>
        //    {
        //        if (ctx.Request.Method == "GET")
        //        {
        //            await SetResultAsync(ctx.Request.QueryString.Value, ctx);
        //        }
        //        else
        //        {
        //            ctx.Response.StatusCode = 405;
        //        }
        //    });
        //}

        private async Task SetResultAsync(string value, HttpContext ctx)
        {
            _source.TrySetResult(value);

            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                ctx.Response.Write("<h1>You can now return to the application.</h1>");
                await ctx.Response.FlushAsync();
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                ctx.Response.Write("<h1>Invalid request.</h1>");
                await ctx.Response.FlushAsync();
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }
}
