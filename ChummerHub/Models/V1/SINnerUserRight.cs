using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("UserRight {EMail}: {CanEdit}")]
    public class SINnerUserRight
    {
        public SINnerUserRight()
        {
        }

        public SINnerUserRight(Guid sinnerId)
        {
            SINnerId = sinnerId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Guid? SINnerId { get; set; }

        private string _email = null;

        [MaxLength(64)]
        public string EMail
        { get { return _email; }
            set
            {
                _email = null;
                _email = value?.ToUpperInvariant();
            }
        }

        public bool CanEdit { get; set; }
    }
}
