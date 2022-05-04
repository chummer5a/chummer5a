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
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class SINSearchGroupResult
    {
        public List<string> Roles { get; set; }

        public ChummerHubVersion Version { get; set; }

        public SINSearchGroupResult()
        {
            SINGroups = new List<SINnerSearchGroup>();
            Roles = new List<string>();
            Version = new ChummerHubVersion();
        }
        public List<SINnerSearchGroup> SINGroups { get; set; }

        public string ErrorText { get; set; }
    }

    public class SINnerSearchGroup : SINnerGroup
    {

        public List<SINnerSearchGroup> MySINSearchGroups { get; set; }
        public string ErrorText { get; set; }
        public List<SINnerSearchGroupMember> MyMembers { get; set; }
        public bool IsFavorite { get; set; }
        public SINnerSearchGroup()
        {
            MyMembers = new List<SINnerSearchGroupMember>();
            MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
        }
        public SINnerSearchGroup(SINnerGroup groupbyname, ApplicationUser user)
        {
            MyParentGroupId = groupbyname?.MyParentGroupId;
            Id = groupbyname?.Id;
            if (groupbyname != null)
                IsPublic = groupbyname.IsPublic;
            Groupname = groupbyname?.Groupname;
            MyMembers = new List<SINnerSearchGroupMember>();
            MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
            MyAdminIdentityRole = groupbyname?.MyAdminIdentityRole;
            Language = groupbyname?.Language;
            PasswordHash = groupbyname?.PasswordHash;
            MySettings = groupbyname?.MySettings;
            HasPassword = PasswordHash?.Length > 0;
            IsFavorite = false;
            if (user != null)
            {
                if (user.FavoriteGroups.Count > 0)
                    user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();
                IsFavorite = user.FavoriteGroups.Any(a => a.FavoriteGuid == Id);
            }
        }

    }

    public class SINnerSearchGroupMember
    {
        public SINner MySINner { get; set; }
        public string Username { get; set; }
        public bool IsFavorite { get; set; }

        public SINnerSearchGroupMember(ApplicationUser user, SINner member)
        {
            //if (user == null)
            //    throw new ArgumentNullException(nameof(user));
            Username = user?.UserName;
            MySINner = member ?? throw new ArgumentNullException(nameof(member));
            if (user  != null && user.FavoriteGroups?.Count > 0)
            {
                user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();
                IsFavorite = user.FavoriteGroups.Any(a => a.FavoriteGuid == MySINner.Id);
            }
        }
        public SINnerSearchGroupMember()
        {
            MySINner = new SINner();
        }

    }

}

