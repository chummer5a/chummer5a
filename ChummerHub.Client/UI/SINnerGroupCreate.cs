using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SINners.Models;
using Chummer.Plugins;
using ChummerHub.Client.Backend;

namespace ChummerHub.Client.UI
{
    public partial class SINnerGroupCreate : UserControl
    {
        public SINnerGroup MyGroup = null;
        public bool EditMode = false;
        public SINnerGroupCreate(SINnerGroup group, bool editMode)
        {
            MyGroup = group;
            EditMode = editMode;
            InitializeComponent();
            InitializeMe();
        }

        public SINnerGroupCreate()
        {
            InitializeComponent();
        }

        public void InitializeMe()
        {
            if (MyGroup != null)
            {
                tbAdminRole.Text = MyGroup.MyAdminIdentityRole;
                tbGroupId.Text = MyGroup.Id?.ToString();
                tbGroupname.Text = MyGroup.Groupname;
                tbLanguage.Text = MyGroup.Language;
                tbParentGroupId.Text = MyGroup.MyParentGroup?.ToString();
                tbPassword.Text = "";
            }
            tbAdminRole.Enabled = false;
            tbGroupId.Enabled = false;
            tbGroupname.Enabled = EditMode;
            tbLanguage.Enabled = EditMode;
            tbParentGroupId.Enabled = false;
            tbPassword.Enabled = EditMode;

            if (StaticUtils.UserRoles?.Contains("GroupAdmin") == true)
            {
                tbAdminRole.Enabled = EditMode;
                tbGroupId.Enabled = false;
                tbParentGroupId.Enabled = EditMode;
            }
            
        }

        private void BOk_Click(object sender, EventArgs e)
        {
            var group = SaveValues(MyGroup);
        }

        private SINnerGroup SaveValues(SINnerGroup myGroup)
        {
            if (myGroup == null)
                myGroup = new SINnerGroup();

            myGroup.Groupname = this.tbGroupname.Text;
            Guid id = Guid.Empty;
            if (Guid.TryParse(this.tbGroupId.Text, out id))
                myGroup.Id = id;
            //if (Guid.TryParse(this.tbParentGroupId.Text, out id))
            //    myGroup.MyParentGroupId = id;

            return myGroup;

        }
    }
}
