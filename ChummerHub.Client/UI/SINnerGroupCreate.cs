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
using Chummer;


namespace ChummerHub.Client.UI
{
    public partial class SINnerGroupCreate : UserControl
    {
        public SINnerGroup MyGroup = null;
        public bool EditMode = false;
        private string _strSelectedLanguage = GlobalOptions.Language;

        public SINnerGroupCreate(SINnerGroup group, bool editMode, bool onlyPWHash)
        {
            MyGroup = group;
            EditMode = editMode;
            InitializeComponent();
            InitializeMe(onlyPWHash);
        }

        public SINnerGroupCreate()
        {
            InitializeComponent();
        }

        public void InitializeMe(bool onlyPWHash)
        {
            if (MyGroup != null)
            {
                tbAdminRole.Text = MyGroup.MyAdminIdentityRole;
                tbGroupId.Text = MyGroup.Id?.ToString();
                tbGroupname.Text = MyGroup.Groupname;
                tbParentGroupId.Text = MyGroup.MyParentGroup?.ToString();
                tbPassword.Text = "";
            }
            tbAdminRole.Enabled = false;
            tbGroupId.Enabled = false;
            tbGroupname.Enabled = EditMode;
            cboLanguage1.Enabled = EditMode;
            tbParentGroupId.Enabled = false;
            tbPassword.Enabled = EditMode;

            if (StaticUtils.UserRoles?.Contains("GroupAdmin") == true)
            {
                tbAdminRole.Enabled = EditMode;
                tbGroupId.Enabled = false;
                tbParentGroupId.Enabled = EditMode;
            }

            this.cboLanguage1 = frmViewer.PopulateLanguageList(cboLanguage1, null);
            imgLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            if (onlyPWHash)
            {
                tbAdminRole.Enabled = false;
                tbGroupId.Enabled = false;
                tbGroupname.Enabled = false;
                cboLanguage1.Enabled = false;
                tbParentGroupId.Enabled = false;
                tbPassword.Enabled = true;
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
            else
                myGroup.Id = Guid.NewGuid();
            if (Guid.TryParse(this.tbParentGroupId.Text, out id))
                myGroup.MyParentGroupId = id;
            myGroup.Password = tbPassword.Text;
            myGroup.Language = cboLanguage1.SelectedItem.ToString();

            return myGroup;

        }

        private void CboLanguage1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            _strSelectedLanguage = cboLanguage1.SelectedValue?.ToString() ?? GlobalOptions.DefaultLanguage;
            imgLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            bool isEnabled = !string.IsNullOrEmpty(_strSelectedLanguage) && _strSelectedLanguage != GlobalOptions.DefaultLanguage;

        }
    }
}
