using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid Id { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }
        
        public Guid? ParentTagId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        public Tag ParentTag { get; set; }

        //[IgnoreDataMember]
        //[JsonIgnore]
        //[XmlIgnore]
        //[NotMapped]
        //public SINner SINner { get; set; }

        public Guid? SINnerId { get; set; }

        public List<Tag> Tags { get; set; }

        public bool IsUserGenerated { get; set; }

        public TagValueEnum TagType { get; set; }

        public enum TagValueEnum
        {
            @list,
            @bool,
            @int,
            @Guid,
            @string,
            @double,
            @binary,
            @enum,
            @other,
            @unknown
        }

        public Tag()
        {
            TagConstructor(null, null);
        }

        public Tag(SINner sinner, Tag parent)
        {
            TagConstructor(sinner, parent);
        }

        private Tag TagConstructor(SINner sinner, Tag parent)
        {
            if (sinner != null)
                this.SINnerId = sinner.Id;
            this.ParentTag = parent;
            this.TagName = "";
            this.TagValue = "";
            this.ParentTagId = Guid.Empty;
            if (parent != null)
                this.ParentTagId = parent.Id;
            this.Tags = new List<Tag>();
            this.TagType = TagValueEnum.unknown;
            IsUserGenerated = false;
            return this;
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        [NotMapped]
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
                    tempParent = tempParent.ParentTag;
                }
                return str;
            }

        }

        internal void SetSinnerIdRecursive(Guid? id)
        {
            this.SINnerId = id;
            foreach(var child in this.Tags)
                child.SetSinnerIdRecursive(id);
        }
    }
}
