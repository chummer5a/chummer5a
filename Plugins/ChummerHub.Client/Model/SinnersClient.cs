using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Sinners
{
    public class MySinnersClient : SinnersClient
    {
        public MySinnersClient(string baseUrl, System.Net.Http.HttpClient httpClient): base (baseUrl, httpClient)
        {

        }

        //public override void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        //{
        //    settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        //}

        protected override Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            try
            {
                return base.ReadObjectResponseAsync<T>(response, headers);
            }
            catch(Exception ae)
            {
                this.ReadResponseAsString = !ReadResponseAsString;
                return base.ReadObjectResponseAsync<T>(response, headers);
            }
            
        }

        
    }

    //interface IMyInterface
    //{
    //    void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings);
    //}
}
