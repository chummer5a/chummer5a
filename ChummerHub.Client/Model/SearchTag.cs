using Chummer;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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

        public SearchTag(PropertyInfo myPropertyInfo, Chummer.HubTagAttribute hubTag)
        {
            MyPropertyInfo = myPropertyInfo;
            MyRuntimeHubTag = hubTag;
            this.SearchTags = new List<SearchTag>();
        }

        public SearchTag(PropertyInfo myPropertyInfo, Chummer.HubClassTagAttribute hubClassTag)
        {
            MyPropertyInfo = myPropertyInfo;
            MyRuntimeHubClassTag = hubClassTag;
            this.SearchTags = new List<SearchTag>();
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

        public String DisplayText
        {
            get
            {
                return this.TagName + " " + this.SearchOpterator + " " + this.TagValue;
            }
        }
    }
}
