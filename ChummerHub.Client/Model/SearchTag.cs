using Chummer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SINners.Models
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
