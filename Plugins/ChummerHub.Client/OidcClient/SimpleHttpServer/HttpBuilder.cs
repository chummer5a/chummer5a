using SimpleHttpServer.Models;
using System.IO;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            string content = File.ReadAllText("Resources/Pages/500.html"); 

            return new HttpResponse()
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse NotFound()
        {
            string content = File.ReadAllText("Resources/Pages/404.html");

            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                ContentAsUTF8 = content
            };
        }
    }
}
