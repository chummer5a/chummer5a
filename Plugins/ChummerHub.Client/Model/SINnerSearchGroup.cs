using System.Collections.Generic;

namespace ChummerHub.Client.Sinners
{
    public partial class SINnerSearchGroup
    {

        public SINnerSearchGroup(SINnerGroup myGroup)
        {
            if (myGroup != null)
            {

                MyMembers = new List<SINnerSearchGroupMember>();

                Id = myGroup.Id;
                MyParentGroupId = myGroup.MyParentGroupId;
                IsPublic = myGroup.IsPublic;
                MySettings = myGroup.MySettings;
                Groupname = myGroup.Groupname;
                //PasswordHash = myGroup.PasswordHash;
                Language = myGroup.Language;
                MyGroups = myGroup.MyGroups;
                MyAdminIdentityRole = myGroup.MyAdminIdentityRole;
            }
        }

        public override string ToString()
        {
            return GroupDisplayname;
        }

        public string GroupDisplayname
        {
            get
            {
                string ret = Groupname;
                if (!string.IsNullOrEmpty(Language))
                {
                    //if ((this.MyMembers != null)
                    //    && (this.MyMembers.Count > 0))
                    //{
                    //    ret += ": " + MyMembers.Count + " members";
                    //}
                }
                return ret;
            }
        }
    }
}
