using ChummerHub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public SINnerSearchGroup()
        {
            MyMembers = new List<SINnerSearchGroupMember>();
            this.MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
        }

        public SINnerSearchGroup(SINnerGroup groupbyname)
        {
            this.MyParentGroupId = groupbyname?.MyParentGroupId;
            this.Id = groupbyname?.Id;
            if (groupbyname != null)
                this.IsPublic = groupbyname.IsPublic;
            this.Groupname = groupbyname?.Groupname;
            MyMembers = new List<SINnerSearchGroupMember>();
            this.MyGroups = new List<SINnerGroup>();
            MySINSearchGroups = new List<SINnerSearchGroup>();
            this.MyAdminIdentityRole = groupbyname.MyAdminIdentityRole;
            this.Language = groupbyname.Language;
            this.PasswordHash = groupbyname.PasswordHash;
            this.MySettings = groupbyname.MySettings;
        }
        
    }

    public class SINnerSearchGroupMember
    {
        public SINner MySINner { get; set; }

        public string Username { get; set; }

        public SINnerSearchGroupMember()
        {
            MySINner = new SINner();
        }

    }

}

