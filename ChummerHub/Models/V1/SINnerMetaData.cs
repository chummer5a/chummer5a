using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SINnerMetaData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid SINnerMetaDataId { get; set; }
        public List<Tag> Tags { get; set; }

        public SINnerMetaData()
        {
            this.Tags = new List<Tag>();
        }

        public List<SINnerComment> Comments { get; } 
    }
}
