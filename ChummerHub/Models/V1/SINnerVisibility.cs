using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    public class SINnerVisibility
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? SINnerVisibilityId { get; set; }

        public bool IsPublic { get; set; }

        public bool IsGroupVisible { get; set; }

        public string Groupname { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [SoapIgnore]
        private string JsonCanEditClientGuids { get; set; }

        [NotMapped]
        public List<Guid> CanEditClientGuids
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(JsonCanEditClientGuids);
            }
            set
            {
                JsonCanEditClientGuids = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [SoapIgnore]
        private string JsonIsVisibleToUserGuids { get; set; }

        [NotMapped]
        public List<Guid> IsVisibleToUserGuids
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(JsonIsVisibleToUserGuids);
            }
            set
            {
                JsonIsVisibleToUserGuids = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        [SoapIgnore]
        private string JsonCanEditUserGuids { get; set; }


        [NotMapped]
        public List<Guid> CanEditUserGuids
    {
            get
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(JsonCanEditUserGuids);
            }
            set
            {
                JsonCanEditUserGuids = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }

        public SINnerVisibility()
        {
            CanEditClientGuids = new List<Guid>();

            IsVisibleToUserGuids = new List<Guid>();

            CanEditUserGuids = new List<Guid>();
        }
    }
}
