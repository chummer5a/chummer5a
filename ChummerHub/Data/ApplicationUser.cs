using ChummerHub.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.AspNetCore.Identity
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser'
    public class ApplicationUser : IdentityUser<Guid>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser'
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser.FavoriteGroups'
        public List<ApplicationUserFavoriteGroup> FavoriteGroups
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser.FavoriteGroups'
        {
            get => LazyLoader?.Load(this, ref _FavoriteGroups);
            set => _FavoriteGroups = value;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser.ApplicationUser()'
        public ApplicationUser()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUser.ApplicationUser()'
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
