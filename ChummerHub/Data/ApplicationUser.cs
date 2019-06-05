using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ChummerHub.Models.V1;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// A way for a GM to search for all the characters of "his" group
        /// </summary>
        [PersonalData]
        [Obsolete]
        //[NotMapped]
        [JsonIgnore]
        [XmlIgnore]
        public string Groupname { get; set; }

        public List<SINnerGroup> FavoriteGroups { get; set; }

        public ApplicationUser()
        {
            FavoriteGroups = new List<SINnerGroup>();
        }

    }
}
