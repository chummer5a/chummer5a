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
using Chummer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ChummerHub.Client.Sinners
{
    public enum EnumSSearchOpterator
    {
        bigger,
        smaller,
        equal,
        contains
    }

    public partial class SearchTag
    {
        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public SearchTag MyParentTag { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public object MyRuntimePropertyValue { get; set; }

        public SearchTag(PropertyInfo myPropertyInfo, HubTagAttribute hubTag)
        {
            MyPropertyInfo = myPropertyInfo;
            MyRuntimeHubTag = hubTag;
            SearchTags = new List<SearchTag>();
        }

        public SearchTag(PropertyInfo myPropertyInfo, HubClassTagAttribute hubClassTag)
        {
            MyPropertyInfo = myPropertyInfo;
            MyRuntimeHubClassTag = hubClassTag;
            SearchTags = new List<SearchTag>();
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public PropertyInfo MyPropertyInfo { get;  set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public HubClassTagAttribute MyRuntimeHubClassTag { get;  set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public HubTagAttribute MyRuntimeHubTag { get;  set; }

        public string DisplayText => TagName + " " + SearchOpterator + " " + TagValue;

        public string SearchOperator { get; set; }
    }
}
