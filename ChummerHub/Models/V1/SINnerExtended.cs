using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("SINnerExtended {Id}")]
    public class SINnerExtended
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public Guid? Id { get; set; }

        [JsonIgnore]
        public SINner MySINner;

        public String JsonSummary { get; set; }

        public SINnerExtended()
        {
            JsonSummary = "";
            MySINner = null;
            Id = Guid.Empty;
        }

    }
}
