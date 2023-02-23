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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChummerHub.Client.Sinners
{
    public partial class SinnersClient
    {
        private readonly List<string> errors = new List<string>();

        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
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
