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
