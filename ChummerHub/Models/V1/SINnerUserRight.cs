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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight'
    public class SINnerUserRight
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerUserRight()'
        public SINnerUserRight()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerUserRight()'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerUserRight(Guid)'
        public SINnerUserRight(Guid sinnerId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerUserRight(Guid)'
        {
            SINnerId = sinnerId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.Id'

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerId'
        public Guid? SINnerId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.SINnerId'

        private string _email = null;

        [MaxLength(64)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.EMail'
        public string EMail
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.EMail'
        {
            get { return _email; }
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
