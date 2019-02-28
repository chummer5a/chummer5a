using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SINnerComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid? Id { get; set; }

        public Guid? SINnerId { get; set; }

        public DateTime MyDateTime { get; }

        public string Comment { get; set; }

        public string Email { get; set; }

        public int Downloads { get; set; }
    }
}
