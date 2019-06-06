using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        private List<ApplicationUserFavoriteGroup> _FavoriteGroups;

        public List<ApplicationUserFavoriteGroup> FavoriteGroups
        {
            get => LazyLoader?.Load(this, ref _FavoriteGroups);
            set => _FavoriteGroups = value;
        }

        public ApplicationUser()
        {
            FavoriteGroups = new List<ApplicationUserFavoriteGroup>();
        }

        private ILazyLoader LazyLoader { get; set; }

        private ApplicationUser(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

    }
}
