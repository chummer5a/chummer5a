using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("Tag {TagComment}: {TagName} ({TagValue})")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag'
    public class Tag
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.Id'
        public Guid Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.Id'

        [MaxLength(64)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagName'
        public string TagName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagName'

        [MaxLength(64)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValue'
        public string TagValue { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValue'


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueFloat'
        public float? TagValueFloat { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueFloat'

        /// <summary>
        /// This has NO FUNCTION and is only here for Debugging reasons.
        /// </summary>
        ///
        [MaxLength(64)]
        public string TagComment { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.ParentTagId'
        public Guid? ParentTagId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.ParentTagId'

        [IgnoreDataMember]
        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.ParentTag'
        public Tag ParentTag { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.ParentTag'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.SINnerId'
        public Guid? SINnerId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.SINnerId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tags'
        public List<Tag> Tags { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tags'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.IsUserGenerated'
        public bool IsUserGenerated { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.IsUserGenerated'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagType'
        public TagValueEnum TagType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagType'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum'
        public enum TagValueEnum
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum'
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.list'
            @list,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.list'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.bool'
            @bool,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.bool'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.int'
            @int,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.int'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.Guid'
            @Guid,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.Guid'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.string'
            @string,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.string'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.double'
            @double,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.double'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.binary'
            @binary,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.binary'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.enum'
            @enum,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.enum'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.other'
            @other,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.other'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.unknown'
            @unknown
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.TagValueEnum.unknown'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tag()'
        public Tag()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tag()'
        {
            TagConstructor(null, null);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tag(SINner, Tag)'
        public Tag(SINner sinner, Tag parent)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.Tag(SINner, Tag)'
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
            this.TagValueFloat = null;
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Tag.Display'
        public string Display
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Tag.Display'
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
            foreach (var child in this.Tags)
                child.SetSinnerIdRecursive(id);
        }
    }
}
