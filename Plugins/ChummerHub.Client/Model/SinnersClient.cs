using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChummerHub.Client.Sinners
{
    public partial class SinnersClient
    {
        private readonly List<string> errors = new List<string>();

        partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateParseHandling = DateParseHandling.None;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            //settings.Error = delegate (object sender, ErrorEventArgs args)
            //{
            //    errors.Add(args.ErrorContext.Error.Message);
            //    args.ErrorContext.Handled = true;
            //};
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
