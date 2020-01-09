using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{
    class MyCredentials : ServiceClientCredentials
    {
        //public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{
        //    //request.Headers.Add("Bearer", "123456");
        //    return base.ProcessHttpRequestAsync(request, cancellationToken);
        //}
    }
}
