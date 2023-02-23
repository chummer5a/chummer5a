// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ChummerHub.Client.Properties;
using IdentityModel.OidcClient;
using NLog;
using SimpleHttpServer.Models;

namespace SimpleHttpServer
{
    public class HttpProcessor
    {

        #region Fields

        private const int MaxPostSize = 10 * 1024 * 1024; // 10MB

        private readonly List<Route> _routes = new List<Route>();

        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        #endregion

        #region Constructors

        #endregion

        #region Public Methods
        public void HandleClient(TcpClient tcpClient)
        {
            using (Stream inputStream = GetInputStream(tcpClient))
            using (Stream outputStream = GetOutputStream(tcpClient))
            {
                HttpRequest request = GetRequest(inputStream, outputStream);

                // route and handle the request...
                HttpResponse response = RouteRequest(inputStream, outputStream, request);

                if (response.Headers.TryGetValue("Autorization", out string token))
                {
                    string pureToken = token.TrimStart("Bearer ".ToCharArray());
                    Settings.Default.BearerToken = pureToken;
                    Settings.Default.Save();
                }

                Log.Info("{0} {1}", response.StatusCode, request.Url);
                // build a default response for errors
                if (response.Content == null)
                {
                    if (response.StatusCode != "200")
                    {
                        response.ContentAsUTF8 = $"{response.StatusCode} {request.Url} <p> {response.ReasonPhrase}";
                    }
                }
                else
                {
                    if (request.Headers.TryGetValue("Cookie", out string cookiestring))
                        OidcClient.SetCookieContainer(cookiestring);
                }

                WriteResponse(outputStream, response);

                outputStream.Flush();
            }
        }

        // this formats the HTTP response...
        private static void WriteResponse(Stream stream, HttpResponse response)
        {            
            if (response.Content == null) {           
                response.Content = new byte[]{};
            }
            
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type")) {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, $"HTTP/1.0 {response.StatusCode} {response.ReasonPhrase}\r\n");
            Write(stream, string.Join("\r\n", response.Headers.Select(x => $"{x.Key}: {x.Value}")));
            Write(stream, "\r\n\r\n");

            stream.Write(response.Content, 0, response.Content.Length);       
        }

        public void AddRoute(Route route)
        {
            _routes.Add(route);
        }

        #endregion

        #region Private Methods

        private static string Readline(Stream stream)
        {
            string data = "";
            while (true)
            {
                int nextChar = stream.ReadByte();
                if (nextChar == '\n') { break; }
                switch (nextChar)
                {
                    case '\r':
                        continue;
                    case -1:
                        Thread.Sleep(1);
                        continue;
                    default:
                        data += Convert.ToChar(nextChar);
                        break;
                }
            }
            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual HttpResponse RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {
            try
            {
                List<Route> routes = _routes.Where(x => x.UrlRegex  == null || Regex.Match(request.Url, x.UrlRegex).Success).ToList();

                if (routes.Count == 0)
                    return HttpBuilder.NotFound();

                Route route = routes.SingleOrDefault(x => x.Method == request.Method);

                if (route == null)
                {
                    return new HttpResponse
                    {
                        ReasonPhrase = "Method Not Allowed",
                        StatusCode = "405"
                    };
                }

                if (route.UrlRegex == null)
                {
                    request.Path = request.Url;
                }
                else
                {
                    // extract the path if there is one
                    Match match = Regex.Match(request.Url, route.UrlRegex);
                    request.Path = match.Groups.Count > 1 ? match.Groups[1].Value : request.Url;
                }

                // trigger the route handler...
                request.Route = route;
                try
                {
                    return route.Callable(request);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return HttpBuilder.InternalServerError();
                }
            }
            catch(Exception ex2)
            {
                Log.Error(ex2);
                return HttpBuilder.InternalServerError();
            }
        }

        private HttpRequest GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new InvalidOperationException("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Trim().Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new InvalidOperationException("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }


            return new HttpRequest
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content
            };
        }

        #endregion


    }
}
