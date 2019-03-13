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

namespace SINners.Models
{
    public partial class SINnerVisibility
    {

        private BindingList<SINerUserRight> _UserRightsObservable = null;

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public BindingList<SINerUserRight> UserRightsObservable
        {
            get
            {
                if (_UserRightsObservable == null)
                {
                    _UserRightsObservable = new BindingList<SINerUserRight>(UserRights);
                }
                return _UserRightsObservable;
            }
            set
            {
                _UserRightsObservable = value;
            }
        }

        public void Save(CheckedListBox clbVisibilityToUsers)
        {
            var test = ChummerHub.Client.Properties.Settings.Default.SINnerVisibility = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            ChummerHub.Client.Properties.Settings.Default.Save();
            if (clbVisibilityToUsers != null)
            {
                for (int i = 0; i < clbVisibilityToUsers.Items.Count; i++)
                {
                    SINerUserRight obj = (SINerUserRight)clbVisibilityToUsers.Items[i];
                    clbVisibilityToUsers.SetItemChecked(i, obj.CanEdit.Value);
                }
            }
        }
        
    }
}
