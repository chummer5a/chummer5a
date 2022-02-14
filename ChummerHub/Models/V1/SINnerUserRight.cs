using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
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

        private string _email;

        [MaxLength(64)]
        public string EMail
        {
            get => _email;
            set
            {
                _email = null;
                _email = value?.ToUpperInvariant();
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.CanEdit'
        public bool CanEdit { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.CanEdit'
    }
}
