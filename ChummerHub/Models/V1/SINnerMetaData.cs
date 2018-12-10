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

        public Guid Id { get; set; }

        public SINnerVisibility Visibility { get; set; }

        public List<Tag> Tags { get; set; }

        public SINnerMetaData()
        {
            this.Tags = new List<Tag>();
            Visibility = new SINnerVisibility();
        }
        
    }
}
