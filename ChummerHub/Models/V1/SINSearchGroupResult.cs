using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;

namespace ChummerHub.Models.V1
{
    public class SINSearchGroupResult
    {
        public List<String> Roles { get; set; }

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
            this.MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
        }
        public SINnerSearchGroup(SINnerGroup groupbyname, ApplicationUser user)
        {
            
            this.MyParentGroupId = groupbyname?.MyParentGroupId;
            this.Id = groupbyname?.Id;
            if (groupbyname != null)
                this.IsPublic = groupbyname.IsPublic;
            this.Groupname = groupbyname?.Groupname;
            MyMembers = new List<SINnerSearchGroupMember>();
            this.MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
            this.MyAdminIdentityRole = groupbyname?.MyAdminIdentityRole;
            this.Language = groupbyname?.Language;
            this.PasswordHash = groupbyname?.PasswordHash;
            this.MySettings = groupbyname?.MySettings;
            this.HasPassword = this.PasswordHash?.Any() == true;
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (user.FavoriteGroups.Any() == false)
                user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();
            if (user.FavoriteGroups.Any(a => a.FavoriteGuid == this.Id))
                IsFavorite = true;
            else
                IsFavorite = false;
        }

    }

    public class SINnerSearchGroupMember
    {
        public SINner MySINner { get; set; }
        public string Username { get; set; }
        public bool IsFavorite { get; set; }

        public SINnerSearchGroupMember(ApplicationUser user, SINner member)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            Username = user.UserName;
            MySINner = member ?? throw new ArgumentNullException(nameof(member));
            if (user.FavoriteGroups.Any() == false)
                user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();
            if (user.FavoriteGroups.Any(a => a.FavoriteGuid == MySINner.Id))
                IsFavorite = true;
            else
                IsFavorite = false;
        }
        public SINnerSearchGroupMember()
        {
            MySINner = new SINner();
        }

    }

}

