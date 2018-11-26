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
        public Guid? Id { get; set; }

        public bool IsPublic { get; set; }

        public bool IsGroupVisible { get; set; }

        public string Groupname { get; set; }

        //[JsonIgnore]
        //[XmlIgnore]
        //[SoapIgnore]
        //private string JsonCanEditClientGuids { get; set; }

        
        public List<SINerUserRight> UserRights { get; set; }

        public SINnerVisibility()
        {
            UserRights = new List<SINerUserRight>();
         
        }
    }
}
