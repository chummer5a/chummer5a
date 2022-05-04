/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
