using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Chummer;
using Newtonsoft.Json;

namespace SINners.Models
{
    public partial class SINnerVisibility
    {

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
                        _UserRightsObservable = new BindingList<SINnerUserRight>(UserRights);
                }
                return _UserRightsObservable;
            }
            set => _UserRightsObservable = value;
        }

        public void AddVisibilityForEmail(string email)
        {
            if (!IsValidEmail(email))
            {
                Program.MainForm.ShowMessageBox("Please enter a valid email address!");
                return;
            }
            SINnerUserRight ur = UserRightsObservable.FirstOrDefault(a => email != null && a != null && a.EMail != null && a.EMail.Equals(email, StringComparison.OrdinalIgnoreCase)) ?? new SINnerUserRight
            {
                EMail = email,
                CanEdit = true,
                Id = Guid.NewGuid()
            };
            if (!UserRightsObservable.Contains(ur))
                UserRightsObservable.Add(ur);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
