using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SINners.Models
{
    [System.Diagnostics.DebuggerDisplay("{Display}")]
    public partial class Tag
    {
        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Tag MyParentTag { get; set; }

        public Tag (Object myRuntimeObject, Chummer.helpers.HubTagAttribute hubTag)
        {
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubTag = hubTag;
            this.Tags = new List<Tag>();
        }

        public Tag (bool isUserGenerated)
        {
            this.IsUserGenerated = isUserGenerated;
            this.Tags = new List<Tag>();
        }

        public Tag(Object myRuntimeObject, Chummer.helpers.HubClassTagAttribute hubClassTag)
        {
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubClassTag = hubClassTag;
            this.Tags = new List<Tag>();
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public string Display
        {
            get
            {
                string str = "";
                Tag tempParent = this;
                while (tempParent != null)
                {
                    string tempstr = tempParent.TagName;
                    if (!String.IsNullOrEmpty(tempParent.TagValue))
                        tempstr += ": " + tempParent.TagValue;
                    if (!String.IsNullOrEmpty(str))
                        tempstr += " -> " + str;
                    str = tempstr;
                    tempParent = tempParent.MyParentTag;
                }
                return str;
            }
            
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public bool DeleteIfEmpty { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Object MyRuntimeObject { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Chummer.helpers.HubTagAttribute MyRuntimeHubTag { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Chummer.helpers.HubClassTagAttribute MyRuntimeHubClassTag { get; set; }

        
    }
}
