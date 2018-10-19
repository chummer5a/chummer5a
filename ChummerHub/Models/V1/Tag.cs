using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid TagId { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }
        
        public Guid? ParentTagId { get; set; }

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
            this.TagName = "";
            this.TagValue = "";
            this.ParentTagId = Guid.Empty;
            this.Tags = new List<Tag>();
            this.TagType = TagValueEnum.unknown;
            IsUserGenerated = false;
        }
    }
}
