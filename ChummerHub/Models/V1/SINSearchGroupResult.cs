using ChummerHub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SINSearchGroupResult
    {
        public SINSearchGroupResult()
        {
            SINGroups = new List<SINnerSearchGroup>();
        }
        public List<SINnerSearchGroup> SINGroups { get; set; }

        public string ErrorText { get; set; }
    }

    public class SINnerSearchGroup : SINnerGroup
    {
        public List<SINnerSearchGroupMember> MyMembers { get; set; }

        public SINnerSearchGroup()
        {
            MyMembers = new List<SINnerSearchGroupMember>();
        }

        public SINnerSearchGroup(SINnerGroup groupbyname)
        {
            this.IsPublic = groupbyname.IsPublic;
            this.Groupname = groupbyname.Groupname;
            MyMembers = new List<SINnerSearchGroupMember>();
        }
        
    }

    public class SINnerSearchGroupMember
    {
        public SINner MySINner { get; set; }

        public string Username { get; set; }

    }

}

