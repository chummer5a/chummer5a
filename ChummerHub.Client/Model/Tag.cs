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

        public Tag (object myRuntimeObject, Chummer.HubTagAttribute hubTag)
        {
            Id = Guid.NewGuid();
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubTag = hubTag;
            Tags = new List<Tag>();
        }

        public Tag (bool isUserGenerated)
        {
            Id = Guid.NewGuid();
            IsUserGenerated = isUserGenerated;
            Tags = new List<Tag>();
        }

        public Tag(object myRuntimeObject, Chummer.HubClassTagAttribute hubClassTag)
        {
            Id = Guid.NewGuid();
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubClassTag = hubClassTag;
            Tags = new List<Tag>();
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
                if(!String.IsNullOrEmpty(this.TagComment))
                    str += " (" + this.TagComment + ")";
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
        public object MyRuntimeObject { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Chummer.HubTagAttribute MyRuntimeHubTag { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Chummer.HubClassTagAttribute MyRuntimeHubClassTag { get; set; }

        internal void SetSinnerIdRecursive(Guid? id)
        {
            SiNnerId = id;
            foreach(var childtag in Tags)
                childtag.SetSinnerIdRecursive(id);
        }

    }
}
