using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("Tag {TagComment}: {TagName} ({TagValue})")]
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid Id { get; set; }

        [MaxLength(64)]
        public string TagName { get; set; }

        [MaxLength(64)]
        public string TagValue { get; set; }


        public float? TagValueFloat { get; set; }

        /// <summary>
        /// This has NO FUNCTION and is only here for Debugging reasons.
        /// </summary>
        ///
        [MaxLength(64)]
        public string TagComment { get; set; }

        public Guid? ParentTagId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        public Tag ParentTag { get; set; }

        public Guid? SINnerId { get; set; }

        public List<Tag> Tags { get; set; }

        public bool IsUserGenerated { get; set; }

        public TagValueEnum TagType { get; set; }

        public enum TagValueEnum
        {
            list,
            @bool,
            @int,
            Guid,
            @string,
            @double,
            binary,
            @enum,
            other,
            unknown
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
                SINnerId = sinner.Id;
            ParentTag = parent;
            TagName = string.Empty;
            TagValue = string.Empty;
            TagValueFloat = null;
            ParentTagId = Guid.Empty;
            if (parent != null)
                ParentTagId = parent.Id;
            Tags = new List<Tag>();
            TagType = TagValueEnum.unknown;
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
                StringBuilder sbdReturn = new StringBuilder();
                Tag tempParent = this;
                while (tempParent != null)
                {
                    string tempstr = tempParent.TagName;
                    if (!string.IsNullOrEmpty(tempParent.TagValue))
                        tempstr += ": " + tempParent.TagValue;
                    if (sbdReturn.Length > 0)
                        tempstr += " -> ";
                    sbdReturn.Insert(0, tempstr);
                    tempParent = tempParent.ParentTag;
                }
                return sbdReturn.ToString();
            }

        }

        internal void SetSinnerIdRecursive(Guid? id)
        {
            SINnerId = id;
            foreach (var child in Tags)
                child.SetSinnerIdRecursive(id);
        }
    }
}
