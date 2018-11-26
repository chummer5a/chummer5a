using Chummer.helpers;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public SearchTag(Object myRuntimeObject, Chummer.helpers.HubTagAttribute hubTag)
        {
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubTag = hubTag;
            this.STags = new List<SearchTag>();
        }

        public SearchTag(Object myRuntimeObject, Chummer.helpers.HubClassTagAttribute hubClassTag)
        {
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubClassTag = hubClassTag;
            this.STags = new List<SearchTag>();
        }

        public object MyRuntimeObject { get;  set; }
        public HubClassTagAttribute MyRuntimeHubClassTag { get;  set; }
        public HubTagAttribute MyRuntimeHubTag { get;  set; }

        public String DisplayText
        {
            get
            {
                return this.STagName + " " + this.SSearchOpterator + " " + this.STagValue;
            }
        }
    }
}
