// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System.Collections.Generic;
using System.Linq;

namespace SimpleHttpServer.Models
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; } // either the Url, or the first regex group
        public string Content { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Content) && !Headers.ContainsKey("Content-Length"))
                Headers.Add("Content-Length", Content.Length.ToString());

            return
                $"{Method} {Url} HTTP/1.0\r\n{string.Join("\r\n", Headers.Select(x => $"{x.Key}: {x.Value}"))}\r\n\r\n{Content}";
        }
    }
}
