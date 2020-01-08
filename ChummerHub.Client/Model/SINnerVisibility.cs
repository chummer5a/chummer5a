using ChummerHub.Client.Backend;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Chummer;

namespace SINners.Models
{
    public partial class SINnerVisibility
    {

        private BindingList<SINnerUserRight> _UserRightsObservable = null;

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
            set
            {
                _UserRightsObservable = value;
            }
        }

        public void AddVisibilityForEmail(string email)
        {
            if (!IsValidEmail(email))
            {
                Program.MainForm.ShowMessageBox("Please enter a valid email address!");
                return;
            }
            SINnerUserRight ur = new SINnerUserRight()
            {
                EMail = email,
                CanEdit = true,
                Id = Guid.NewGuid()
            };
            var found = from a in this.UserRightsObservable where email != null && a!= null && a.EMail != null &&  a.EMail?.ToLowerInvariant() == email.ToLowerInvariant() select a;
            if (found.Any())
                ur = found.FirstOrDefault();
            if (!this.UserRightsObservable.Contains(ur))
                this.UserRightsObservable.Add(ur);

        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
