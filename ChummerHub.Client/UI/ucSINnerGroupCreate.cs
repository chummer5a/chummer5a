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
    public partial class ucSINnerGroupCreate : UserControl
    {
        public SINnerGroup MyGroup = null;
        public bool EditMode = false;
        private string _strSelectedLanguage = GlobalOptions.Language;

        public ucSINnerGroupCreate(SINnerGroup group, bool editMode, bool onlyPWHash)
        {
            MyGroup = group;
            EditMode = editMode;
            InitializeComponent();
            InitializeMe(onlyPWHash);
        }

        public ucSINnerGroupCreate()
        {
            InitializeComponent();
        }

        public void InitializeMe(bool onlyPWHash)
        {
            cbIsPublic.Checked = false;
            if (StaticUtils.UserRoles?.Contains("GroupAdmin") == true)
            {
                EditMode = true;
            }
            this.cboLanguage1 = frmViewer.PopulateLanguageList(cboLanguage1, null);
            imgLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            if (MyGroup != null)
            {
                tbDescription.Text = MyGroup.Description;
                tbAdminRole.Text = MyGroup.MyAdminIdentityRole;
                tbGroupId.Text = MyGroup.Id?.ToString();
                tbGroupname.Text = MyGroup.Groupname;
                tbParentGroupId.Text = MyGroup.MyParentGroupId?.ToString();
                tbPassword.Text = "";
                if (!String.IsNullOrEmpty(MyGroup.GroupCreatorUserName))
                    tbGroupCreatorUsername.Text = MyGroup.GroupCreatorUserName;
                else
                    tbGroupCreatorUsername.Text = Properties.Settings.Default.UserEmail;
                if (!String.IsNullOrEmpty(MyGroup.Language))
                    this.cboLanguage1.SelectedValue = MyGroup.Language;
                else
                {
                    this.cboLanguage1.SelectedValue = GlobalOptions.Language;
                }
                if (MyGroup.IsPublic.HasValue)
                    cbIsPublic.Checked = MyGroup.IsPublic.Value;
            }

            tbGroupCreatorUsername.ReadOnly = true;
            tbAdminRole.ReadOnly = true;
            tbGroupId.ReadOnly = true;
            tbGroupname.ReadOnly = !EditMode;
            cboLanguage1.Enabled = EditMode;
            cbIsPublic.Enabled = EditMode;
            tbPassword.ReadOnly = !EditMode;

            if (StaticUtils.UserRoles?.Contains("GroupAdmin") == true)
            {
                tbAdminRole.ReadOnly = !EditMode;
                tbGroupId.ReadOnly = true;
                tbGroupCreatorUsername.ReadOnly = !EditMode;
            }

           
            

            if (onlyPWHash)
            {
                tbAdminRole.ReadOnly = true;
                tbGroupId.ReadOnly = true;
                tbGroupname.ReadOnly = true;
                cboLanguage1.Enabled = false;
                tbParentGroupId.ReadOnly = true;
                tbPassword.ReadOnly = false;
                tbGroupCreatorUsername.Enabled = false;
                cbIsPublic.Enabled = false;
                tbDescription.Enabled = false;
                this.ActiveControl = tbPassword;
            }
            if (this.ParentForm != null)
                this.ParentForm.AcceptButton = bOk;

        }

        private void BOk_Click(object sender, EventArgs e)
        {
            MyGroup = SaveValues(MyGroup);
        }

        private SINnerGroup SaveValues(SINnerGroup myGroup)
        {
            if (myGroup == null)
                myGroup = new SINnerGroup();

            myGroup.Groupname = this.tbGroupname.Text;
            Guid id = Guid.Empty;
            myGroup.Id = Guid.TryParse(this.tbGroupId.Text, out id) ? id : Guid.NewGuid();
            if (Guid.TryParse(this.tbParentGroupId.Text, out id))
                myGroup.MyParentGroupId = id;
            myGroup.Password = tbPassword.Text;
            if (String.IsNullOrEmpty(tbPassword.Text))
                myGroup.PasswordHash = null;
            myGroup.Language = cboLanguage1.SelectedItem.ToString();
            myGroup.IsPublic = cbIsPublic.Checked;
            myGroup.MyAdminIdentityRole = tbAdminRole.Text;
            myGroup.GroupCreatorUserName = tbGroupCreatorUsername.Text;
            myGroup.Description = tbDescription.Text;
            return myGroup;

        }

        private void CboLanguage1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            _strSelectedLanguage = cboLanguage1.SelectedValue?.ToString() ?? GlobalOptions.DefaultLanguage;
            imgLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            bool isEnabled = !string.IsNullOrEmpty(_strSelectedLanguage) && _strSelectedLanguage != GlobalOptions.DefaultLanguage;

        }

        private void TbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BOk_Click(this, new EventArgs());
            }
        }

        private void bParentGroupId_Click(object sender, EventArgs e)
        {
            var text = LanguageManager.GetString("String_SINners_HowToGroupParentDescription", _strSelectedLanguage);
            this.DoThreadSafe(() =>
            {
                PluginHandler.MainForm.ShowMessageBox(this.Parent, text, "How to ...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
    }
}
