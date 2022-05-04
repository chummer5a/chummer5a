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
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Chummer;
using Newtonsoft.Json;

namespace ChummerHub.Client.Sinners
{
    public partial class SINnerVisibility
    {
        public SINnerVisibility()
        {
            this.IsGroupVisible = true;
        }

        private BindingList<SINnerUserRight> _UserRightsObservable;

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public BindingList<SINnerUserRight> UserRightsObservable
        {
            get
            {
                if (_UserRightsObservable == null)
                {
                    if (UserRights == null)
                        UserRights = new List<SINnerUserRight>();
                    if (UserRights != null)
                        _UserRightsObservable = new BindingList<SINnerUserRight>(UserRights.ToArray());
                }
                return _UserRightsObservable;
            }
            set => _UserRightsObservable = value;
        }

        public void AddVisibilityForEmail(string email)
        {
            if (!IsValidEmail(email))
            {
                Program.ShowMessageBox("Please enter a valid email address!");
                return;
            }
            SINnerUserRight ur = UserRightsObservable.FirstOrDefault(a => a?.EMail != null && a.EMail.Equals(email, StringComparison.OrdinalIgnoreCase)) ?? new SINnerUserRight
            {
                EMail = email,
                CanEdit = true,
                Id = Guid.NewGuid()
            };
            if (!UserRightsObservable.Contains(ur))
            {
                UserRights.Add(ur);
                UserRightsObservable = null;
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
