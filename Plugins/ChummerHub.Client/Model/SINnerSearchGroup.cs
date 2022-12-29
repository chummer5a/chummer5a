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
