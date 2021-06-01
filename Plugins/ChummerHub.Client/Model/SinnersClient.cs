using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChummerHub.Client.Sinners
{
    public partial class SinnersClient
    {
        
        partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.DateParseHandling = DateParseHandling.None;
            settings.Converters.Add(new FixedIsoDateTimeOffsetConverter());
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        

        //protected override Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        //{
        //    try
        //    {
        //        return base.ReadObjectResponseAsync<T>(response, headers);
        //    }
        //    catch(Exception ae)
        //    {
        //        this.ReadResponseAsString = !ReadResponseAsString;
        //        return base.ReadObjectResponseAsync<T>(response, headers);
        //    }
            
        //}

        
    }

    //interface IMyInterface
    //{
    //    void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings);
    //}
}
